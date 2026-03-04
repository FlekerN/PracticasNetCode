using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine;

namespace Networking.Cliente
{
    public static class AuthenticationWrapper
    {
        public enum AuthState
        {
            NotAuthenticated,
            Authenticating,
            Authenticated,
            Error,
            Timeout
        }

        public static AuthState State { get; private set; } = AuthState.NotAuthenticated;
        public static Exception LastException { get; private set; }

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

            if (IsSessionActive())
            {
                SetState(AuthState.Authenticated);
                return State;
            }

            SetState(AuthState.Authenticating);

            using var timeoutCts = (timeoutMs > 0) ? new CancellationTokenSource(timeoutMs) : null;
            using var linkedCts = timeoutCts != null
                ? CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token)
                : CancellationTokenSource.CreateLinkedTokenSource(ct);

            var token = linkedCts.Token;

            for (int attempt = 1; attempt <= Mathf.Max(1, maxTries); attempt++)
            {
                token.ThrowIfCancellationRequested();

                try
                {
                    await EnsureUnityServicesInitialized(token);

                    if (IsSessionActive())
                    {
                        SetState(AuthState.Authenticated);
                        return State;
                    }

                    await AuthenticationService.Instance.SignInAnonymouslyAsync();

                    if (IsSessionActive())
                    {
                        SetState(AuthState.Authenticated);
                        return State;
                    }

                    LastException = new Exception("SignIn completed but session is not active (IsSignedIn/IsAuthorized false).");
                }
                catch (OperationCanceledException oce)
                {
                    LastException = oce;
                    SetState(AuthState.Timeout);
                    return State;
                }
                catch (Exception ex)
                {
                    LastException = ex;
                }

                if (attempt < maxTries)
                {
                    try
                    {
                        await Task.Delay(retryDelayMs, token);
                    }
                    catch (OperationCanceledException oce)
                    {
                        LastException = oce;
                        SetState(AuthState.Timeout);
                        return State;
                    }
                }
            }

            // agotó reintentos
            SetState((timeoutCts != null && timeoutCts.IsCancellationRequested) ? AuthState.Timeout : AuthState.Error);
            return State;
        }

        public static bool IsSessionActive()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                return false;

            return AuthenticationService.Instance.IsSignedIn &&
                   AuthenticationService.Instance.IsAuthorized;
        }

        private static async Task EnsureUnityServicesInitialized(CancellationToken ct)
        {
            if (UnityServices.State == ServicesInitializationState.Initialized)
                return;

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

            await UnityServices.InitializeAsync();
        }
    }
}