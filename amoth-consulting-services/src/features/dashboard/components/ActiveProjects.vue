<template>
  <section class="active-projects">
    <h2 v-if="showHeader" class="section-title">Active Projects</h2>

    <div v-if="projects.length === 0" class="empty">
      <p>No active projects available right now. Add one to get started.</p>
    </div>

    <div v-else class="project-table-wrapper">
      <table class="project-table" aria-label="List of active projects">
        <thead>
          <tr>
            <th>Project</th>
            <th>Status</th>
            <th>Due Date</th>
            <th>Progress</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="project in projects" :key="project.id">
            <td>{{ project.name }}</td>
            <td><span :class="`status-pill ${statusClass(project.status)}`">{{ project.status || 'Unknown' }}</span></td>
            <td>{{ formatDate(project.endDate) }}</td>
            <td>
              <div class="progress-track" role="progressbar" :aria-valuenow="progress(project)" aria-valuemin="0" aria-valuemax="100">
                <div class="progress-fill" :style="{ width: `${progress(project)}%` }"></div>
              </div>
              <small>{{ progress(project) }}%</small>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="project-actions">
      <button @click="openAll" class="btn-view-all">View Full Project Dashboard</button>
    </div>
  </section>
</template>

<script setup>
import { useRouter } from 'vue-router'

defineProps({
  showHeader: { type: Boolean, default: true },
  projects: { type: Array, default: () => [] },
})

const router = useRouter()

function progress(project) {
  const rate = Number(project.completionRate)
  return Number.isFinite(rate) ? Math.min(100, Math.max(0, Math.round(rate))) : 0
}

function formatDate(date) {
  if (!date) return 'Not set'
  const value = new Date(date)
  return Number.isNaN(value.getTime()) ? 'Not set' : value.toLocaleDateString()
}

function statusClass(status) {
  return String(status || 'unknown').replace(/\s+/g, '-').toLowerCase()
}

const openAll = () => {
  router.push('/projects')
}
</script>

<style scoped>
.active-projects {
  background: var(--color-background-soft);
  border: 1px solid var(--color-border);
  border-radius: 10px;
  padding: 1.25rem;
  margin-top: 1.5rem;
  box-shadow: 0 6px 18px rgba(2, 6, 23, 0.04);
}

.section-title {
  margin: 0 0 1rem;
  color: var(--color-heading);
  font-size: 1.4rem;
}

.empty {
  color: var(--color-text);
  padding: 1rem;
  background: var(--color-background);
  border: 1px solid var(--color-border);
  border-radius: 10px;
}

.project-table-wrapper {
  overflow-x: auto;
}

.project-table {
  width: 100%;
  border-collapse: separate;
  border-spacing: 0 0.75rem;
}

.project-table th,
.project-table td {
  padding: 0.85rem 0.9rem;
  text-align: left;
  color: var(--color-text);
  vertical-align: middle;
}

.project-table th {
  font-size: 0.9rem;
  color: var(--color-heading);
  font-weight: 600;
  border-bottom: 1px solid var(--color-border);
}

.project-table tbody tr {
  background: var(--color-background);
  border: 1px solid var(--color-border);
  border-radius: 12px;
}

.status-pill {
  display: inline-flex;
  padding: 0.25rem 0.5rem;
  border-radius: 999px;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.03em;
}

.status-pill.on-track { background: #e6f8ed; color: #0c7a3f; }
.status-pill.in-progress { background: #fff4e6; color: #b87a00; }
.status-pill.at-risk { background: #fff3f0; color: #b02a1d; }
.status-pill.on-hold { background: #eff4ff; color: #1d4ed8; }

.progress-track {
  width: 100%;
  height: 10px;
  background: #eaeaea;
  border-radius: 999px;
  overflow: hidden;
  margin-bottom: 0.3rem;
}

.progress-fill {
  background: linear-gradient(90deg, #38b2ac, #2c5282);
  height: 100%;
  transition: width 0.35s ease-in-out;
}

.project-actions {
  margin-top: 1rem;
  text-align: right;
}

.btn-view-all {
  background: #2b6cb0;
  border: none;
  color: #fff;
  border-radius: 6px;
  padding: 0.55rem 1rem;
  cursor: pointer;
  transition: background 0.2s ease;
}

.btn-view-all:hover {
  background: #1e4f7f;
}

.project-table small {
  color: #606060;
}
</style>
