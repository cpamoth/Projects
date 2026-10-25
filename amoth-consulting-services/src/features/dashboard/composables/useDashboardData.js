import { computed, ref } from 'vue'
import { completeTask, getMeetings, getProjects, getTasks } from '../../../services/dashboardApi.js'

function getErrorMessage(error) {
  return error instanceof Error ? error.message : 'Unable to load dashboard data'
}

export function useDashboardData() {
  const projects = ref([])
  const tasks = ref([])
  const meetings = ref([])
  const isLoading = ref(false)
  const hasLoaded = ref(false)
  const errorMessage = ref('')
  const updatingTaskId = ref(null)

  const activeProjects = computed(() => projects.value.filter((project) => {
    const status = String(project.status || '').toLowerCase()
    return status === 'active' || status === 'in progress'
  }))

  const pendingTasks = computed(() => tasks.value.filter((task) => (
    task.completed !== true &&
    !['completed', 'complete', 'done'].includes(String(task.status || '').toLowerCase())
  )))

  const upcomingMeetings = computed(() => meetings.value
    .filter((meeting) => (
      String(meeting.status || '').toLowerCase() === 'scheduled' &&
      Number.isFinite(Date.parse(meeting.scheduledDate)) &&
      Date.parse(meeting.scheduledDate) >= Date.now()
    ))
    .sort((first, second) => Date.parse(first.scheduledDate) - Date.parse(second.scheduledDate)))

  const completionRate = computed(() => {
    if (tasks.value.length === 0) return 0
    const completedCount = tasks.value.filter((task) => (
      task.completed === true ||
      ['completed', 'complete', 'done'].includes(String(task.status || '').toLowerCase())
    )).length
    return Math.round((completedCount / tasks.value.length) * 100)
  })

  async function refreshDashboard() {
    isLoading.value = true
    errorMessage.value = ''

    try {
      const [projectResults, taskResults, meetingResults] = await Promise.all([
        getProjects(),
        getTasks(),
        getMeetings(),
      ])
      if (![projectResults, taskResults, meetingResults].every(Array.isArray)) {
        throw new Error('Dashboard data response was invalid')
      }
      projects.value = projectResults
      tasks.value = taskResults
      meetings.value = meetingResults
      hasLoaded.value = true
      return true
    } catch (error) {
      errorMessage.value = getErrorMessage(error)
      return false
    } finally {
      isLoading.value = false
    }
  }

  async function markTaskComplete(taskId) {
    updatingTaskId.value = taskId
    errorMessage.value = ''

    try {
      await completeTask(taskId)
      await refreshDashboard()
    } catch (error) {
      errorMessage.value = getErrorMessage(error)
    } finally {
      updatingTaskId.value = null
    }
  }

  return {
    activeProjects,
    completionRate,
    errorMessage,
    hasLoaded,
    isLoading,
    meetings,
    pendingTasks,
    projects,
    refreshDashboard,
    markTaskComplete,
    upcomingMeetings,
    tasks,
    updatingTaskId,
  }
}
