import { apiRequest as request, apiUpload } from './apiClient';

// Owner + Admin: the platform's Sham Cash wallet (owners pay their commission to it).
export async function getPlatformSettings() {
  const d = await request('/api/v1/platform-settings');
  return {
    shamCashWallet: d?.shamCashWallet || '',
    shamCashQrUrl: d?.shamCashQrUrl || '',
  };
}

// Admin only.
export async function updatePlatformShamCash(wallet) {
  return request('/api/v1/platform-settings/shamcash', {
    method: 'PUT',
    body: JSON.stringify({ shamCashWallet: wallet || null }),
  });
}

// Admin only.
export async function uploadPlatformShamCashQr(file) {
  const formData = new FormData();
  formData.append('file', file);
  return apiUpload('/api/v1/platform-settings/shamcash-qr/upload', formData);
}
