import { apiRequest } from './apiClient.js'

export const getProfile = () => apiRequest('/api/profile')

export const updateProfile = (profile) => apiRequest('/api/profile', {
  method: 'PUT',
  body: JSON.stringify(profile)
})
