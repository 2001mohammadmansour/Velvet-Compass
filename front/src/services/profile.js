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
  };
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
