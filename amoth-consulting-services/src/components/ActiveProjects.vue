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
            <th>Client</th>
            <th>Status</th>
            <th>Due Date</th>
            <th>Progress</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="project in projects" :key="project.id">
            <td>{{ project.name }}</td>
            <td>{{ project.client }}</td>
            <td><span :class="`status-pill ${project.status.replace(/\s+/g, '-').toLowerCase()}`">{{ project.status }}</span></td>
            <td>{{ project.dueDate }}</td>
            <td>
              <div class="progress-track" role="progressbar" :aria-valuenow="project.progress" aria-valuemin="0" aria-valuemax="100">
                <div class="progress-fill" :style="{ width: `${project.progress}%` }"></div>
              </div>
              <small>{{ project.progress }}%</small>
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
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const props = defineProps({
  showHeader: { type: Boolean, default: true }
})

const router = useRouter()
const defaultProjects = [
  { id: 1, name: 'Platform migration', client: 'Acme Corp', status: 'In Progress', dueDate: '2026-06-30', progress: 60 },
  { id: 2, name: 'Operational analytics', client: 'Globex', status: 'At Risk', dueDate: '2026-05-14', progress: 45 },
  { id: 3, name: 'Security roadmap', client: 'Initech', status: 'On Track', dueDate: '2026-07-19', progress: 75 },
  { id: 4, name: 'Digital transformation', client: 'Umbrella', status: 'On Hold', dueDate: '2026-09-01', progress: 30 }
]

const projects = ref(defaultProjects)

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
