// CHANGED BY AI (2026-07-13): new service file for the self-service Edit Profile feature.
import { apiRequest as request } from './apiClient';

export async function getMyProfile() {
  const p = await request('/api/v1/auth/me');
  return {
    id: p.id,
    username: p.username,
    email: p.email,
    phoneNumber: p.phoneNumber || '',
    role: p.role,
    createdAt: p.createdAt,
    twoFactorEnabled: !!p.twoFactorEnabled,
  };
}

// ─── Two-factor authentication (self-service, from the Edit Profile page) ──────

// Step 1 of enabling: returns { manualEntryKey, qrCodeImageBase64 }. The QR string already
// carries the "data:image/png;base64," prefix, so it can go straight into an <img src>.
export async function setupTwoFactor() {
  return request('/api/v1/auth/2fa/setup', { method: 'POST' });
}

// Step 2: confirm the 6-digit code from the authenticator app. Returns { recoveryCodes: [...] }
// which must be shown to the user once.
export async function enableTwoFactor(code) {
  return request('/api/v1/auth/2fa/enable', {
    method: 'POST',
    body: JSON.stringify({ code }),
  });
}

export async function disableTwoFactor(password) {
  return request('/api/v1/auth/2fa/disable', {
    method: 'POST',
    body: JSON.stringify({ password }),
  });
}

export async function regenerateRecoveryCodes() {
  return request('/api/v1/auth/2fa/recovery-codes/regenerate', { method: 'POST' });
}

export async function updateMyProfile({ username, phoneNumber }) {
  const p = await request('/api/v1/auth/me', {
    method: 'PUT',
    body: JSON.stringify({ username, phoneNumber: phoneNumber || null }),
  });
  return {
    id: p.id,
    username: p.username,
    email: p.email,
    phoneNumber: p.phoneNumber || '',
    role: p.role,
    createdAt: p.createdAt,
  };
}

export async function changeMyPassword({ currentPassword, newPassword }) {
  return request('/api/v1/auth/change-password', {
    method: 'POST',
    body: JSON.stringify({ currentPassword, newPassword }),
  });
}

// Keeps the locally-cached session in sync so the navbar/profile dropdown reflects a username
// change immediately, without needing to log out and back in.
export function updateStoredUsername(username) {
  try {
    const raw = localStorage.getItem('mock_auth_user');
    const parsed = raw ? JSON.parse(raw) : {};
    localStorage.setItem('mock_auth_user', JSON.stringify({
      ...parsed,
      user: { ...(parsed?.user || {}), username },
    }));
  } catch (error) { /* ignore */ }
}
