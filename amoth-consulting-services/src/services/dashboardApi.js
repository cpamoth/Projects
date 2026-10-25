import { apiRequest } from './apiClient.js'

export const getProjects = () => apiRequest('/api/projects')
export const getTasks = () => apiRequest('/api/tasks')
export const getMeetings = () => apiRequest('/api/meetings')

export const completeTask = (taskId) => apiRequest(`/api/tasks/${encodeURIComponent(taskId)}`, {
  method: 'PUT',
  body: JSON.stringify({ completed: true, status: 'Completed' }),
})
