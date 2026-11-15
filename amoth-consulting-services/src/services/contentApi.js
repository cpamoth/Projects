import { services as fallbackServices } from '../data/services.js'

const apiUrl = (import.meta.env.VITE_API_URL || '').replace(/\/$/, '')

const requestApi = async (path) => {
  if (!apiUrl) {
    return null
  }

  const response = await fetch(`${apiUrl}${path}`)
  if (!response.ok) {
    throw new Error(`API request failed: ${response.status}`)
  }

  return response.json()
}

export const getServices = async () => {
  if (!apiUrl) {
    return fallbackServices
  }

  try {
    return (await requestApi('/api/services')) || fallbackServices
  } catch (error) {
    console.warn('Unable to load services from the API. Using local fallback.', error)
    return fallbackServices
  }
}

export const getServiceById = async (serviceId) => {
  const localService = fallbackServices.find((service) => service.id === serviceId)

  if (!apiUrl) {
    return localService || fallbackServices[0]
  }

  try {
    return (await requestApi(`/api/services/${encodeURIComponent(serviceId)}`)) || localService || fallbackServices[0]
  } catch (error) {
    console.warn('Unable to load service from the API. Using local fallback.', error)
    return localService || fallbackServices[0]
  }
}