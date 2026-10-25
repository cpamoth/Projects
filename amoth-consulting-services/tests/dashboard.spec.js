import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises, mount } from '@vue/test-utils'
import Dashboard from '../src/features/dashboard/components/Dashboard.vue'
import { completeTask, getMeetings, getProjects, getTasks } from '../src/services/dashboardApi.js'

vi.mock('../src/services/dashboardApi.js', () => ({
  completeTask: vi.fn(),
  getMeetings: vi.fn(),
  getProjects: vi.fn(),
  getTasks: vi.fn(),
}))

vi.mock('../src/services/authApi.js', () => ({
  getAuthUser: () => ({ fullName: 'Jane Doe', username: 'jane' }),
}))

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: vi.fn() }),
  RouterLink: {
    props: ['to'],
    template: '<a :href="to"><slot /></a>',
  },
}))

const projectData = [
  { id: 1, name: 'Website redesign', status: 'Active', endDate: '2027-01-10', completionRate: 50 },
  { id: 2, name: 'Archived project', status: 'Completed', endDate: null, completionRate: 100 },
]

const taskData = [
  { id: 10, title: 'Write content', projectId: 1, status: 'Pending', completed: false, dueDate: '2027-01-05' },
  { id: 11, title: 'Approve design', projectId: 1, status: 'Completed', completed: true, dueDate: null },
]

const meetingData = [
  { id: 20, title: 'Design review', scheduledDate: '2027-01-08T14:00:00.000Z', status: 'Scheduled' },
  { id: 21, title: 'Past meeting', scheduledDate: '2020-01-01T14:00:00.000Z', status: 'Scheduled' },
  { id: 22, title: 'Cancelled meeting', scheduledDate: '2027-01-08T14:00:00.000Z', status: 'Cancelled' },
]

const global = {
  stubs: {
    PageWrapper: { template: '<div><slot /></div>' },
    RouterLink: {
      props: ['to'],
      template: '<a :href="to"><slot /></a>',
    },
  },
}

describe('live dashboard', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-01-01T00:00:00.000Z'))
    vi.mocked(getProjects).mockResolvedValue(projectData)
    vi.mocked(getTasks).mockResolvedValue(taskData)
    vi.mocked(getMeetings).mockResolvedValue(meetingData)
    vi.mocked(completeTask).mockResolvedValue({ message: 'Task updated successfully' })
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  it('loads live records and calculates dashboard counts and completion rate', async () => {
    const wrapper = mount(Dashboard, { global })
    await flushPromises()

    expect(getProjects).toHaveBeenCalledOnce()
    expect(getTasks).toHaveBeenCalledOnce()
    expect(getMeetings).toHaveBeenCalledOnce()
    expect(wrapper.text()).toContain('Jane Doe')
    expect(wrapper.text()).toContain('50%')
    expect(wrapper.text()).toContain('Website redesign')
    expect(wrapper.text()).toContain('Write content')
    expect(wrapper.text()).toContain('Design review')
    expect(wrapper.text()).not.toContain('Past meeting')
    expect(wrapper.text()).not.toContain('Cancelled meeting')
    expect(wrapper.text()).not.toContain('Archived project')
  })

  it('persists task completion and reloads dashboard data', async () => {
    vi.mocked(getTasks)
      .mockResolvedValueOnce(taskData)
      .mockResolvedValueOnce(taskData.map((task) => (
        task.id === 10 ? { ...task, completed: true, status: 'Completed' } : task
      )))

    const wrapper = mount(Dashboard, { global })
    await flushPromises()
    await wrapper.get('.btn-done').trigger('click')
    await flushPromises()

    expect(completeTask).toHaveBeenCalledWith(10)
    expect(getProjects).toHaveBeenCalledTimes(2)
    expect(getTasks).toHaveBeenCalledTimes(2)
    expect(wrapper.text()).not.toContain('Write content')
    expect(wrapper.text()).toContain('100%')
  })

  it('shows an error and offers retry when dashboard data cannot load', async () => {
    vi.mocked(getTasks).mockRejectedValueOnce(new Error('Tasks service unavailable'))

    const wrapper = mount(Dashboard, { global })
    await flushPromises()

    expect(wrapper.get('[role="alert"]').text()).toContain('Tasks service unavailable')
    expect(wrapper.find('.stats-grid').exists()).toBe(false)

    await wrapper.get('[role="alert"] button').trigger('click')
    await flushPromises()
    expect(wrapper.find('.stats-grid').exists()).toBe(true)
  })
})
