<template>
  <nav class="site-nav" aria-label="Main Navigation">
    <a href="/" class="brand" aria-label="Amoth Consulting home">
      <span>Amoth Consulting</span>
    </a>

    <button
      class="menu-toggle"
      type="button"
      @click="toggleMenu"
      :aria-expanded="menuOpen.toString()"
      aria-controls="navigation-menu"
      aria-label="Toggle navigation menu"
    >
      ☰
    </button>

    <ul id="navigation-menu" class="links" :class="{ open: menuOpen }">
      <li><RouterLink to="/">Home</RouterLink></li>
      <li><RouterLink to="/about">About</RouterLink></li>
      <li><RouterLink to="/services">Services</RouterLink></li>
      <li v-if="auth"><RouterLink to="/dashboard">Dashboard</RouterLink></li>
    </ul>

    <div class="actions">
      <div class="search">
        <input placeholder="Search" aria-label="Search site content" />
      </div>

      <button
        class="theme-switcher"
        type="button"
        @click="emit('toggle-theme')"
        :aria-label="props.theme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'"
        :title="props.theme === 'dark' ? 'Light mode' : 'Dark mode'"
      >
        <span v-if="props.theme === 'dark'">☀️</span>
        <span v-else>🌙</span>
      </button>

      <div v-if="!auth" class="auth-links">
        <RouterLink to="/register" class="register-link" aria-label="Register for new account"><UserPlusIcon class="btn-icon" aria-hidden="true" />Register</RouterLink>
        <RouterLink to="/login" class="login-link" aria-label="Sign in to account"><ArrowRightOnRectangleIcon class="btn-icon" aria-hidden="true" />Login</RouterLink>
      </div>

      <div v-else class="user">
        <RouterLink to="/profile" class="profile-link">{{ userDisplayName }}</RouterLink>
        <button class="btn-logout" @click="logout" aria-label="Sign out of account"><ArrowLeftOnRectangleIcon class="btn-icon" aria-hidden="true" />Sign Out</button>
      </div>
    </div>
  </nav>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { UserPlusIcon, ArrowRightOnRectangleIcon, ArrowLeftOnRectangleIcon } from '@heroicons/vue/24/solid'

const props = defineProps({
  theme: {
    type: String,
    default: 'light'
  }
})
const emit = defineEmits(['toggle-theme'])

const router = useRouter()
const route = useRoute()
const tokenUpdate = ref(0)
const menuOpen = ref(false)

const toggleMenu = () => {
  menuOpen.value = !menuOpen.value
}

watch(
  () => route.path,
  () => {
    menuOpen.value = false
  }
)

const auth = computed(() => {
  tokenUpdate.value
  return !!localStorage.getItem('token')
})

const userDisplayName = computed(() => {
  tokenUpdate.value
  const token = localStorage.getItem('token')
  if (token) {
    try {
      const decoded = atob(token)
      const parts = decoded.split(':')
      const username = parts[0]
      const userProfile = localStorage.getItem('userProfile')
      if (userProfile) {
        try {
          const profile = JSON.parse(userProfile)
          if (profile.fullName && profile.fullName.trim()) {
            return profile.fullName
          }
        } catch (e) {
          // fall back to username
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
  window.addEventListener('authStateChanged', () => {
    tokenUpdate.value++
  })
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
.site-nav { position:fixed; top:0; left:0; right:0; z-index:1200; display:flex; align-items:center; justify-content:space-between; padding: .6rem 1rem; border-bottom:1px solid var(--color-border); background:var(--color-background-soft); height:var(--navbar-height, 64px); box-sizing:border-box; box-shadow: 0 2px 8px rgba(2,6,23,0.08); width:100%; }
.brand { text-decoration:none; color: var(--accent); font-weight:700; }
.links { list-style:none; display:flex; gap:1rem; margin:0; padding:0 }
.links a { text-decoration:none; color: hsla(160, 100%, 37%, 1); }
.actions { display:flex; align-items:center; gap:1rem }
.search input { padding:.4rem .6rem; border:1px solid var(--color-border); border-radius:6px; background: var(--color-background); color: var(--color-text); }
.search input::placeholder { color: var(--color-text); opacity: 0.65; }
.theme-switcher {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 44px;
  height: 44px;
  border: 1px solid var(--color-border);
  border-radius: 12px;
  background: var(--color-background);
  color: var(--color-text);
  cursor: pointer;
  transition: transform 180ms ease, background-color 180ms ease, border-color 180ms ease;
}
.theme-switcher:hover,
.theme-switcher:focus-visible {
  transform: translateY(-1px);
  background: var(--color-background-soft);
  border-color: var(--color-border-hover);
}
.theme-switcher span { font-size: 1.05rem; }
.menu-toggle { display: none; border: 1px solid var(--color-border); border-radius: 12px; background: var(--color-background); color: var(--color-text); cursor: pointer; }
.register-link,
.login-link {
  padding: .4rem .6rem;
  border-radius: 6px;
  background: var(--color-background-soft);
  color: var(--accent);
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  gap: .5rem;
  border: 1px solid var(--accent);
}
.auth-links .register-link,
.auth-links .login-link,
.btn-logout {
  display: inline-flex;
  align-items: center;
  gap: .5rem;
}
.btn-icon { display:inline-flex; width:18px; height:18px; align-items:center; justify-content:center; font-size:14px; }
.btn-icon svg { display:block; width:100%; height:100%; color: currentColor; fill: currentColor; stroke: none; }
.register-link:hover,
.login-link:hover { background: var(--color-background); border-color: var(--color-border-hover); filter: brightness(1.02); }
.user { display:flex; gap:.5rem; align-items:center }
.profile-link { text-decoration: none; color: hsla(160, 100%, 37%, 1); font-weight: 600; cursor: pointer; }
.profile-link:hover { opacity: 0.8; }
.btn-logout { padding: .45rem .75rem; border-radius: 6px; background: var(--accent); color: var(--vt-c-white); border: 1px solid transparent; cursor: pointer; }
.btn-logout:hover { filter: brightness(1.05); }
@media (max-width:860px) {
  .site-nav { flex-wrap: wrap; align-items: flex-start; padding: .75rem 1rem; }
  .menu-toggle { display: inline-flex; align-items: center; justify-content: center; width: 44px; height: 44px; }
  .links { width: 100%; display: none; flex-direction: column; gap: 0; margin: 0; padding: 0.75rem 0; background: var(--color-background-soft); border-top: 1px solid var(--color-border); position: relative; left: 0; right: 0; }
  .links.open { display: flex; }
  .actions { width: 100%; margin-top: 0.75rem; justify-content: space-between; }
}
</style>
