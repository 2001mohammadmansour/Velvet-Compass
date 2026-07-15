import { apiRequest as request, apiUpload } from './apiClient';

export async function getPartners() {
  const result = await request('/api/v1/partners');
  return Array.isArray(result) ? result : [];
}

export async function createPartner(partner) {
  return request('/api/v1/partners', {
    method: 'POST',
    body: JSON.stringify({
      name: partner.name,
      city: partner.city,
      description: partner.description,
    }),
  });
}

export async function updatePartner(id, partner) {
  return request(`/api/v1/partners/${id}`, {
    method: 'PUT',
    body: JSON.stringify({
      name: partner.name,
      city: partner.city,
      description: partner.description,
    }),
  });
}

export async function deletePartner(id) {
  return request(`/api/v1/partners/${id}`, { method: 'DELETE' });
}

export async function uploadPartnerPhoto(partnerId, file) {
  const formData = new FormData();
  formData.append('file', file);
  return apiUpload(`/api/v1/partners/${partnerId}/image/upload`, formData);
}
