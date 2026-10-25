<template>
  <section id="scheduled-meetings" class="scheduled-meetings">
    <h2>Scheduled Meetings</h2>

    <div v-if="meetings.length === 0" class="empty">No upcoming meetings scheduled.</div>

    <ul v-else class="meeting-list" aria-label="Scheduled meetings list">
      <li v-for="meeting in meetings" :key="meeting.id" class="meeting-item">
        <div>
          <strong>{{ meeting.title }}</strong>
          <p class="meeting-meta">{{ formatDate(meeting.scheduledDate) }}</p>
        </div>
      </li>
    </ul>

    <div class="meeting-summary" role="status">
      <span>{{ meetings.length }} upcoming meeting{{ meetings.length === 1 ? '' : 's' }}</span>
    </div>
  </section>
</template>

<script setup>
defineProps({
  meetings: { type: Array, default: () => [] },
})

function formatDate(date) {
  const value = new Date(date)
  return Number.isNaN(value.getTime()) ? 'Date unavailable' : value.toLocaleString()
}
</script>

<style scoped>
.scheduled-meetings {
  background: var(--color-background-soft);
  border: 1px solid var(--color-border);
  border-radius: 10px;
  padding: 1.25rem;
  margin-top: 1.5rem;
  box-shadow: 0 6px 18px rgba(2, 6, 23, 0.04);
}

.scheduled-meetings h2 {
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

.meeting-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: grid;
  gap: 0.75rem;
}

.meeting-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border: 1px solid var(--color-border);
  border-radius: 10px;
  padding: 0.75rem;
  gap: 1rem;
  background: var(--color-background);
}

.meeting-meta {
  margin: 0.2rem 0 0;
  color: #666;
  font-size: 0.9rem;
}

.meeting-summary {
  margin-top: 0.75rem;
  color: #4d4d4d;
  font-weight: 600;
}
</style>
