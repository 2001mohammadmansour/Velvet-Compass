import { apiRequest as request } from './apiClient';

// Admin: platform-wide pending vs collected commission, per hotel.
export async function getCommissionOverview() {
  const d = await request('/api/v1/commission/overview');
  return {
    pendingTotal: Number(d?.pendingTotal) || 0,
    collectedTotal: Number(d?.collectedTotal) || 0,
    hotels: Array.isArray(d?.hotels) ? d.hotels : [],
  };
}

// Admin: confirm a hotel's owner-claimed commission payment actually arrived.
export async function confirmCommission(hotelId) {
  return request(`/api/v1/commission/hotel/${hotelId}/confirm`, { method: 'POST' });
}
