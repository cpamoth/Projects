<template>
  <PageWrapper>
    <div class="dashboard">
      <div class="dashboard-header">
        <div>
          <h1 class="green">Welcome back, {{ username }}!</h1>
          <p class="subtitle">Here's what's happening with your account</p>
        </div>
        <button @click="logout" class="logout-btn">Sign Out</button>
      </div>

      <div class="stats-grid">
        <div class="stat-card">
          <div class="stat-number">8</div>
          <div class="stat-label">Active Projects</div>
          <p class="stat-desc">Currently in progress</p>
        </div>
        <div class="stat-card">
          <div class="stat-number">3</div>
          <div class="stat-label">Scheduled Meetings</div>
          <p class="stat-desc">This week</p>
        </div>
        <div class="stat-card">
          <div class="stat-number">12</div>
          <div class="stat-label">Pending Tasks</div>
          <p class="stat-desc">Awaiting action</p>
        </div>
        <div class="stat-card">
          <div class="stat-number">95%</div>
          <div class="stat-label">Completion Rate</div>
          <p class="stat-desc">Across all projects</p>
        </div>
      </div>

      <CompletionRate />
      <ActiveProjects />
      <PendingTasks />
      <ScheduledMeetings />

      <section class="recent-activity">
        <h2 class="green">Quick Next Steps</h2>
        <ul class="action-list">
          <li><a href="#" class="action-link">View Active Projects</a></li>
          <li><a href="#" class="action-link">Schedule a Meeting</a></li>
          <li><a href="#" class="action-link">Review Pending Tasks</a></li>
          <li><a href="/profile" class="action-link">Update Your Profile</a></li>
        </ul>
      </section>
    </div>
  </PageWrapper>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import ActiveProjects from './ActiveProjects.vue'
import PendingTasks from './PendingTasks.vue'
import ScheduledMeetings from './ScheduledMeetings.vue'
import CompletionRate from './CompletionRate.vue'

const router = useRouter()
const username = ref('')

onMounted(() => {
  const token = localStorage.getItem('token')
  if (token) {
    const decodedToken = atob(token)
    const [user] = decodedToken.split(':')
    
    // Try to get full name from user profile
    const userProfile = localStorage.getItem('userProfile')
    if (userProfile) {
      try {
        const profile = JSON.parse(userProfile)
        if (profile.fullName && profile.fullName.trim()) {
          username.value = profile.fullName
        } else {
          username.value = user
        }
      } catch (e) {
        username.value = user
      }
    } else {
      username.value = user
    }
  }
})

function logout() {
  localStorage.removeItem('token')
  router.push('/login')
}
</script>

<style scoped>
.dashboard { 
  max-width: 1200px; 
  margin: 2rem auto; 
  padding: 1rem 
}

.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 3rem;
  gap: 2rem;
}

.dashboard-header h1 {
  margin: 0 0 0.5rem 0;
}

.subtitle {
  color: #666;
  font-size: 1rem;
  margin: 0;
}

.logout-btn { 
  padding: 0.75rem 1.5rem; 
  background: #2b6cb0; 
  color: #fff; 
  border: none; 
  border-radius: 4px;
  font-size: 1rem;
  cursor: pointer;
  transition: background 0.3s;
  white-space: nowrap;
}

.logout-btn:hover {
  background: #1e4f7f;
}

.logout-btn:focus-visible {
  outline: 3px solid #2b6cb0;
  outline-offset: 2px;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1.5rem;
  margin-bottom: 1.5rem;
}

.stat-card {
  background: var(--color-background-soft);
  border: 1px solid var(--color-border);
  border-radius: 10px;
  padding: 1.5rem;
  text-align: center;
  box-shadow: 0 6px 18px rgba(2,6,23,0.04);
  transition: transform 0.3s, box-shadow 0.3s;
}

.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 24px rgba(2,6,23,0.1);
}

.stat-number {
  font-size: 2.5rem;
  font-weight: bold;
  color: hsla(160, 100%, 37%, 1);
  margin-bottom: 0.5rem;
}

.stat-label {
  font-size: 1rem;
  font-weight: 600;
  color: #333;
  margin: 0;
}

.stat-desc {
  font-size: 0.875rem;
  color: #999;
  margin-top: 0.5rem;
  margin-bottom: 0;
}

.recent-activity {
  background: var(--color-background-soft);
  border-radius: 10px;
  padding: 2rem;
  border: 1px solid var(--color-border);
  box-shadow: 0 6px 18px rgba(2, 6, 23, 0.04);
  margin-top: 1.5rem;
}

.recent-activity h2 {
  margin-top: 0;
  margin-bottom: 1.5rem;
  color: var(--color-heading);
}

.action-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: grid;
  gap: 0.85rem;
}

.action-link {
  display: block;
  color: var(--accent);
  text-decoration: none;
  font-weight: 600;
  padding: 0.95rem 1rem;
  border-radius: 10px;
  background: var(--color-background);
  border: 1px solid var(--color-border);
  transition: background 0.3s, transform 0.2s ease;
}

.action-link:hover,
.action-link:focus-visible {
  background: var(--color-background-soft);
  transform: translateX(2px);
  text-decoration: none;
}

.action-link:focus-visible {
  outline: 3px solid #2b6cb0;
  outline-offset: 2px;
}

@media (max-width: 768px) {
  .dashboard-header {
    flex-direction: column;
  }

  .logout-btn {
    align-self: flex-start;
  }

  .stats-grid {
    grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
    gap: 1rem;
  }

  .stat-number {
    font-size: 2rem;
  }

  .stat-card {
    padding: 1rem;
  }
}
</style>
