import { describe, expect, it, vi } from 'vitest'
import { apiRequest } from '../src/services/apiClient.js'
import { completeTask, getMeetings, getProjects, getTasks } from '../src/services/dashboardApi.js'

vi.mock('../src/services/apiClient.js', () => ({
  apiRequest: vi.fn(),
}))

describe('dashboardApi', () => {
  it('requests the authenticated dashboard resources', async () => {
    vi.mocked(apiRequest).mockResolvedValueOnce([]).mockResolvedValueOnce([]).mockResolvedValueOnce([])

    await getProjects()
    await getTasks()
    await getMeetings()

    expect(apiRequest.mock.calls).toEqual([
      ['/api/projects'],
      ['/api/tasks'],
      ['/api/meetings'],
    ])
  })

  it('marks a task complete through the protected task update endpoint', async () => {
    vi.mocked(apiRequest).mockResolvedValueOnce({ message: 'Task updated successfully' })

    await completeTask('task/10')

    expect(apiRequest).toHaveBeenCalledWith('/api/tasks/task%2F10', {
      method: 'PUT',
      body: JSON.stringify({ completed: true, status: 'Completed' }),
    })
  })
})
