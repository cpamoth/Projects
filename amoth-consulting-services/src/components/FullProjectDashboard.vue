<template>
  <PageWrapper>
    <section class="full-dashboard">
      <header class="full-dashboard-header">
        <h1>Full Project Dashboard</h1>
        <p>Overview of active projects, pending tasks, and scheduled meetings in one place.</p>
        <RouterLink to="/dashboard" class="btn-back">Back to Dashboard</RouterLink>
      </header>

      <div class="details-grid">
        <CompletionRate :showHeader="false" />
        <ActiveProjects :showHeader="false" />
        <PendingTasks :showHeader="false" />
        <ScheduledMeetings :showHeader="false" />
      </div>
    </section>
  </PageWrapper>
</template>

<script setup>
import ActiveProjects from './ActiveProjects.vue'
import PendingTasks from './PendingTasks.vue'
import ScheduledMeetings from './ScheduledMeetings.vue'
import CompletionRate from './CompletionRate.vue'
</script>

<style scoped>
.full-dashboard { max-width: 1200px; margin: 2rem auto 3rem; }
.full-dashboard-header { display: flex; flex-direction: column; gap: 0.5rem; margin-bottom: 1.5rem; }
.full-dashboard-header h1 { margin: 0; }
.full-dashboard-header p { margin: 0; color: #575757; }
.btn-back { align-self: flex-start; margin-top: 0.75rem; padding: 0.4rem 0.85rem; border-radius: 6px; background: #2b6cb0; color: #fff; text-decoration: none; }
.btn-back:hover { background: #1e4f7f; }
.details-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 1.5rem; }
@media (max-width: 900px) {
  .details-grid { grid-template-columns: 1fr; }
}
</style>