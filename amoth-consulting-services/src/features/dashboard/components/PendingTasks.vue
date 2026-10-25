<template>
  <section id="pending-tasks" class="pending-tasks">
    <h2>Pending Tasks</h2>

    <div v-if="tasks.length === 0" class="empty">No pending tasks currently. Great job!</div>

    <ul v-else class="tasks-list" aria-label="Pending tasks list">
      <li v-for="task in tasks" :key="task.id" class="task-item">
        <div>
          <strong>{{ task.title }}</strong>
          <p class="task-meta">{{ projectName(task) }} • Due {{ formatDate(task.dueDate) }}</p>
        </div>
        <div class="task-actions">
          <span class="task-status">{{ task.status || 'Pending' }}</span>
          <button
            type="button"
            :disabled="updatingTaskId !== null"
            @click="$emit('mark-done', task.id)"
            class="btn-done"
          >
            {{ updatingTaskId === task.id ? 'Saving…' : 'Mark done' }}
          </button>
        </div>
      </li>
    </ul>
  </section>
</template>

<script setup>
const props = defineProps({
  tasks: { type: Array, default: () => [] },
  projects: { type: Array, default: () => [] },
  updatingTaskId: { type: [Number, String], default: null },
})

defineEmits(['mark-done'])

function projectName(task) {
  const project = props.projects.find((entry) => String(entry.id) === String(task.projectId))
  return project?.name || 'Unassigned project'
}

function formatDate(date) {
  if (!date) return 'No date set'
  const value = new Date(date)
  return Number.isNaN(value.getTime()) ? 'No date set' : value.toLocaleDateString()
}
</script>

<style scoped>
.pending-tasks {
  background: var(--color-background-soft);
  border: 1px solid var(--color-border);
  border-radius: 10px;
  padding: 1.25rem;
  margin-top: 1.5rem;
  box-shadow: 0 6px 18px rgba(2, 6, 23, 0.04);
}

.pending-tasks h2 {
  margin-top: 0;
  font-size: 1.35rem;
  color: var(--color-heading);
}

.empty {
  color: var(--color-text);
  padding: 1rem;
  background: var(--color-background);
  border: 1px solid var(--color-border);
  border-radius: 6px;
}

.tasks-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: grid;
  gap: 0.75rem;
}

.task-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border: 1px solid var(--color-border);
  border-radius: 10px;
  padding: 0.75rem;
  gap: 1rem;
  background: var(--color-background);
}

.task-meta {
  margin: 0.2rem 0 0;
  color: #666;
  font-size: 0.9rem;
}

.task-actions {
  display: flex;
  align-items: center;
  gap: 0.6rem;
}

.task-status {
  padding: 0.2rem 0.6rem;
  border-radius: 999px;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
}

.task-status { background: #fff7e6; color: #8a5a00; }

.btn-done {
  border: none;
  background: #2b6cb0;
  color: white;
  padding: 0.4rem 0.8rem;
  border-radius: 5px;
  cursor: pointer;
  transition: background 0.2s ease;
}

.btn-done:hover {
  background: #1e4f7f;
}

.btn-done:disabled {
  cursor: wait;
  opacity: 0.7;
}
</style>
