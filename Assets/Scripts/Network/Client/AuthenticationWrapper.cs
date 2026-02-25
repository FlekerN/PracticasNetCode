using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine;

namespace Networking.Cliente
{
    /// <summary>
    /// Wrapper estático para autenticar (anónimamente) contra Unity Services sin necesidad de un MonoBehaviour en escena.
    /// Puedes llamar a DoAuth() desde cualquier parte del proyecto.
    /// </summary>
    public static class AuthenticationWrapper
    {
        public enum AuthState
        {
            /// <summary>El jugador todavía NO ha iniciado sesión.</summary>
            NotAuthenticated,

            /// <summary>Se está intentando autenticar (en proceso).</summary>
            Authenticating,

            /// <summary>Inicio de sesión completado con éxito y sesión activa.</summary>
            Authenticated,

            /// <summary>Falló la autenticación (excepción, credenciales, red, etc.).</summary>
            Error,

            /// <summary>Se agotó el tiempo máximo de espera durante el proceso.</summary>
            Timeout
        }

        /// <summary>
        ///     Estado actual. Lectura pública, escritura privada (nadie externo puede cambiarlo).
        /// </summary>
        public static AuthState State { get; private set; } = AuthState.NotAuthenticated;

        /// <summary>
        /// Último error capturado (si lo hay), para depuración o UI.
        /// </summary>
        public static Exception LastException { get; private set; }

        /// <summary>
        /// Autentica anónimamente con reintentos.
        /// - Inicializa Unity Services si hace falta.
        /// - Llama SignInAnonymouslyAsync().
        /// - Verifica IsSignedIn + IsAuthorized.
        /// - Reintenta hasta maxTries, esperando retryDelayMs entre intentos.
        /// - Permite timeout global opcional y cancelación vía token.
        /// </summary>
        public static event Action<AuthState> OnStateChanged;

        private static void SetState(AuthState newState)
        {
            if (State == newState) return;
            State = newState;
            Debug.Log($"[AUTH] State -> {State}");
            OnStateChanged?.Invoke(State);
        }
        public static async Task<AuthState> DoAuth(
            int maxTries = 3,
            int retryDelayMs = 1000,
            int timeoutMs = 15000,
            CancellationToken ct = default)
        {
            LastException = null;

            // Si ya está autenticado, no repetimos trabajo.
            if (IsSessionActive())
            {
                SetState(AuthState.Authenticated);
                return State;
            }

            SetState(AuthState.Authenticating);

            // Timeout global opcional (0 o negativo => sin timeout).
            using var timeoutCts = (timeoutMs > 0) ? new CancellationTokenSource(timeoutMs) : null;
            using var linkedCts = timeoutCts != null
                ? CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token)
                : CancellationTokenSource.CreateLinkedTokenSource(ct);

            var token = linkedCts.Token;

            // Intentos
            for (int attempt = 1; attempt <= Mathf.Max(1, maxTries); attempt++)
            {
                token.ThrowIfCancellationRequested();

                try
                {
                    // 1) Inicializar Unity Services si no lo está
                    await EnsureUnityServicesInitialized(token);

                    // 2) Si ya existe sesión activa, OK
                    if (IsSessionActive())
                    {
                        SetState(AuthState.Authenticated);
                        return State;
                    }

                    // 3) Intentar login anónimo
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();

                    // 4) Verificar sesión realmente activa
                    if (IsSessionActive())
                    {
                        SetState(AuthState.Authenticated);
                        return State;
                    }

                    // Si no quedó activo, consideramos fallo y reintento
                    LastException = new Exception("SignIn completed but session is not active (IsSignedIn/IsAuthorized false).");
                }
                catch (OperationCanceledException oce)
                {
                    LastException = oce;

                    // Si el cancel viene por timeout, marcamos Timeout; si no, también puedes decidir tratarlo aparte.
                    State = (timeoutCts != null && timeoutCts.IsCancellationRequested) ? AuthState.Timeout : AuthState.Timeout;
                    return State;
                }
                catch (Exception ex)
                {
                    LastException = ex;
                }

                // Si no es el último intento, esperamos y reintentamos
                if (attempt < maxTries)
                {
                    try
                    {
                        await Task.Delay(retryDelayMs, token);
                    }
                    catch (OperationCanceledException oce)
                    {
                        LastException = oce;
                        State = (timeoutCts != null && timeoutCts.IsCancellationRequested) ? AuthState.Timeout : AuthState.Timeout;
                        return State;
                    }
                }
            }

            // Se agotaron los reintentos
            State = (timeoutCts != null && timeoutCts.IsCancellationRequested) ? AuthState.Timeout : AuthState.Error;
            return State;
        }

        /// <summary>
        /// Devuelve true si la sesión está activa según Unity Authentication.
        /// </summary>
        public static bool IsSessionActive()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                return false;

            // Ya es seguro acceder a Instance
            return AuthenticationService.Instance.IsSignedIn &&
                   AuthenticationService.Instance.IsAuthorized;
        }

        private static async Task EnsureUnityServicesInitialized(CancellationToken ct)
        {
            // UnityServices.State puede ser Uninitialized / Initializing / Initialized, etc.
            if (UnityServices.State == ServicesInitializationState.Initialized)
                return;

            // Si está inicializando ya, esperamos “en bucle” suave para no lanzar doble init.
            if (UnityServices.State == ServicesInitializationState.Initializing)
            {
                while (UnityServices.State == ServicesInitializationState.Initializing)
                {
                    ct.ThrowIfCancellationRequested();
                    await Task.Delay(50, ct);
                }

                if (UnityServices.State == ServicesInitializationState.Initialized)
                    return;
            }

            // Si no está inicializado, inicializamos
            await UnityServices.InitializeAsync();
        }
    }
}
