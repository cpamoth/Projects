<template>
  <PageWrapper>
    <section class="full-dashboard">
      <header class="full-dashboard-header">
        <h1>Full Project Dashboard</h1>
        <p>Overview of active projects, pending tasks, and scheduled meetings in one place.</p>
        <button class="btn-refresh" type="button" :disabled="isLoading" @click="refreshDashboard">
          {{ isLoading ? 'Refreshing…' : 'Refresh dashboard' }}
        </button>
        <RouterLink to="/dashboard" class="btn-back">Back to Dashboard</RouterLink>
      </header>

      <p v-if="isLoading && !hasLoaded" role="status">Loading your dashboard…</p>
      <div v-if="errorMessage" class="dashboard-error" role="alert">
        <span>{{ errorMessage }}</span>
        <button v-if="!hasLoaded" type="button" @click="refreshDashboard">Try again</button>
      </div>

      <div class="details-grid">
        <CompletionRate
          v-if="hasLoaded"
          :show-header="false"
          :rate="completionRate"
          :completed-count="tasks.length - pendingTasks.length"
          :task-count="tasks.length"
        />
        <ActiveProjects v-if="hasLoaded" :show-header="false" :projects="activeProjects" />
        <PendingTasks
          v-if="hasLoaded"
          :tasks="pendingTasks"
          :projects="projects"
          :updating-task-id="updatingTaskId"
          @mark-done="markTaskComplete"
        />
        <ScheduledMeetings v-if="hasLoaded" :meetings="upcomingMeetings" />
      </div>
    </section>
  </PageWrapper>
</template>

<script setup>
import { onMounted } from 'vue'
import ActiveProjects from '../../dashboard/components/ActiveProjects.vue'
import PendingTasks from '../../dashboard/components/PendingTasks.vue'
import ScheduledMeetings from '../../dashboard/components/ScheduledMeetings.vue'
import CompletionRate from '../../dashboard/components/CompletionRate.vue'
import { useDashboardData } from '../../dashboard/composables/useDashboardData.js'

const {
  activeProjects,
  completionRate,
  errorMessage,
  hasLoaded,
  isLoading,
  pendingTasks,
  projects,
  refreshDashboard,
  markTaskComplete,
  upcomingMeetings,
  tasks,
  updatingTaskId,
} = useDashboardData()

onMounted(refreshDashboard)
</script>

<style scoped>
.full-dashboard { width: 100%; margin: 0 0 .5rem; padding: .5rem; }
.full-dashboard-header { display: flex; flex-direction: column; gap: 0.5rem; margin-bottom: 1.5rem; }
.full-dashboard-header h1 { margin: 0; }
.full-dashboard-header p { margin: 0; color: #575757; }
.btn-refresh, .dashboard-error button { align-self: flex-start; padding: 0.4rem 0.85rem; border: 0; border-radius: 6px; background: #2b6cb0; color: #fff; cursor: pointer; }
.btn-refresh:disabled { cursor: wait; opacity: .7; }
.dashboard-error { display: flex; align-items: center; gap: 1rem; padding: 1rem; border-radius: 8px; background: #fff3f0; color: #8d2118; }
.btn-back { align-self: flex-start; margin-top: 0.75rem; padding: 0.4rem 0.85rem; border-radius: 6px; background: #2b6cb0; color: #fff; text-decoration: none; }
.btn-back:hover { background: #1e4f7f; }
.details-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 1.5rem; }
@media (max-width: 900px) {
  .details-grid { grid-template-columns: 1fr; }
}
</style>