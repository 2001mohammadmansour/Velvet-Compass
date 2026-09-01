import { apiRequest as request, apiUpload } from './apiClient';

// Section slugs shown as tabs / offered in the admin form. The backend keeps an internal
// "Other" fallback for bad payloads, but it is not a browsable section.
export const PARTNER_CATEGORIES = ['CarRental', 'Dining', 'Tours', 'Transport'];

export async function getPartners() {
  const result = await request('/api/v1/partners');
  return Array.isArray(result) ? result : [];
}

function toPayload(partner) {
  return {
    name: partner.name,
    cities: Array.isArray(partner.cities) ? partner.cities : [],
    description: partner.description,
    category: partner.category || 'Other',
    websiteUrl: partner.websiteUrl || null,
  };
}

export async function createPartner(partner) {
  return request('/api/v1/partners', { method: 'POST', body: JSON.stringify(toPayload(partner)) });
}

export async function updatePartner(id, partner) {
  return request(`/api/v1/partners/${id}`, { method: 'PUT', body: JSON.stringify(toPayload(partner)) });
}

export async function deletePartner(id) {
  return request(`/api/v1/partners/${id}`, { method: 'DELETE' });
}

export async function uploadPartnerPhoto(partnerId, file) {
  const formData = new FormData();
  formData.append('file', file);
  return apiUpload(`/api/v1/partners/${partnerId}/image/upload`, formData);
}

// Fire-and-forget: a visitor opened this partner / followed its website link.
export function registerPartnerClick(partnerId) {
  try {
    request(`/api/v1/partners/${partnerId}/click`, { method: 'POST' }).catch(() => {});
  } catch { /* never block navigation on this */ }
}
