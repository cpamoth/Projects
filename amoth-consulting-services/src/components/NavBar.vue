<template>
  <nav class="site-nav" aria-label="Main Navigation">
    <div class="brand">Amoth Consulting</div>
    <ul class="links">
      <li><RouterLink to="/">Home</RouterLink></li>
      <li><RouterLink to="/about">About</RouterLink></li>
      <li><RouterLink to="/services">Services</RouterLink></li>
      <li v-if="auth"><RouterLink to="/dashboard">Dashboard</RouterLink></li>
    </ul>

    <div class="actions">
      <div class="search">
        <input placeholder="Search" aria-label="Search site content" />
      </div>

      <div v-if="!auth" class="auth-links">
        <RouterLink to="/register" class="register-link" aria-label="Register for new account"><UserPlusIcon class="btn-icon" aria-hidden="true" />Register</RouterLink>
        <RouterLink to="/login" class="login-link" aria-label="Sign in to account"><ArrowRightOnRectangleIcon class="btn-icon" aria-hidden="true" />Login</RouterLink>
      </div>

      <div v-else class="user">
        <RouterLink to="/profile" class="profile-link">{{ userEmail }}</RouterLink>
        <button class="btn-logout" @click="logout" aria-label="Sign out of account"><ArrowLeftOnRectangleIcon class="btn-icon" aria-hidden="true" />Sign Out</button>
      </div>
    </div>
  </nav>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { UserPlusIcon, ArrowRightOnRectangleIcon, ArrowLeftOnRectangleIcon } from '@heroicons/vue/24/solid'

const router = useRouter()
const tokenUpdate = ref(0)

const auth = computed(() => {
  tokenUpdate.value // trigger reactivity
  return !!localStorage.getItem('token')
})

const userEmail = computed(() => {
  tokenUpdate.value // trigger reactivity
  const token = localStorage.getItem('token')
  if (token) {
    try {
      const decoded = atob(token)
      const parts = decoded.split(':')
      const username = parts[0] // Username
      
      // Try to get full name from user profile
      const userProfile = localStorage.getItem('userProfile')
      if (userProfile) {
        try {
          const profile = JSON.parse(userProfile)
          if (profile.fullName && profile.fullName.trim()) {
            return profile.fullName
          }
        } catch (e) {
          // Fall back to username if profile parsing fails
        }
      }
      
      return username || 'User'
    } catch (e) {
      return 'User'
    }
  }
  return 'User'
})

onMounted(() => {
  // Listen for auth state changes
  window.addEventListener('authStateChanged', () => {
    tokenUpdate.value++
  })
  // Listen for storage changes in other tabs
  window.addEventListener('storage', () => {
    tokenUpdate.value++
  })
})

function logout() {
  localStorage.removeItem('token')
  tokenUpdate.value++
  router.push('/login')
}
</script>

<style scoped>
.site-nav { position:fixed; top:0; left:0; right:0; z-index:1000; display:flex; align-items:center; justify-content:space-between; padding: .6rem 1rem; border-bottom:1px solid var(--color-border); background:var(--color-background-soft); height:var(--navbar-height, 64px); box-sizing:border-box; box-shadow: 0 2px 8px rgba(2,6,23,0.08); }
.brand { font-weight:700; color: hsla(160, 100%, 37%, 1); }
.links { list-style:none; display:flex; gap:1rem; margin:0; padding:0 }
.links a { text-decoration:none; color: hsla(160, 100%, 37%, 1); }
.actions { display:flex; align-items:center; gap:1rem }
.search input { padding:.4rem .6rem; border:1px solid var(--color-border); border-radius:6px }
  .register-link,
  .login-link {
    padding: .4rem .6rem;
    border-radius: 6px;
    background: #0f1724;
    color: #fff;
    text-decoration: none;
    display: inline-block;
    border: 1px solid rgba(255,255,255,0.06);
  }

  .auth-links .register-link,
  .auth-links .login-link,
  .btn-logout {
    display: inline-flex;
    align-items: center;
    gap: .5rem;
  }

  .btn-icon {
    display:inline-flex;
    width:18px;
    height:18px;
    align-items:center;
    justify-content:center;
    font-size:14px;
  }

  .btn-icon svg {
    display:block;
    width:100%;
    height:100%;
    color: currentColor;
    fill: currentColor;
    stroke: none;
  }

  .register-link:hover,
  .login-link:hover {
    background: #0b1320;
    filter: brightness(1.03);
  }
.user { display:flex; gap:.5rem; align-items:center }
.user-email { font-weight:600 }
.profile-link { text-decoration: none; color: hsla(160, 100%, 37%, 1); font-weight: 600; cursor: pointer; }
.profile-link:hover { opacity: 0.8; }
  .btn-logout {
    padding: .45rem .75rem;
    border-radius: 6px;
    background: #0f1724;
    color: #fff;
    border: 1px solid rgba(255,255,255,0.06);
    cursor: pointer;
  }

  .btn-logout:hover {
    background: #0b1320;
    filter: brightness(1.03);
  }

@media (max-width:600px) {
  .links { display:none }
}
</style>