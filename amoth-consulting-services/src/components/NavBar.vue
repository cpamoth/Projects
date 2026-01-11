<template>
  <nav class="site-nav">
    <div class="brand">Amoth Consulting</div>
    <ul class="links">
      <li><RouterLink to="/">Home</RouterLink></li>
      <li><RouterLink to="/about">About</RouterLink></li>
      <li><RouterLink to="/services">Services</RouterLink></li>
      <li v-if="auth"><RouterLink to="/dashboard">Dashboard</RouterLink></li>
    </ul>

    <div class="actions">
      <div class="search">
        <input placeholder="Search" aria-label="Search" />
      </div>

      <div v-if="!auth" class="auth-links">
        <RouterLink to="/register" class="register-link"><UserPlusIcon class="btn-icon" aria-hidden="true" />Register</RouterLink>
        <RouterLink to="/login" class="login-link"><ArrowRightOnRectangleIcon class="btn-icon" aria-hidden="true" />Login</RouterLink>
      </div>

      <div v-else class="user">
        <span class="user-email">{{ userEmail }}</span>
        <button class="btn-logout" @click="logout"><ArrowLeftOnRectangleIcon class="btn-icon" aria-hidden="true" />Sign Out</button>
      </div>
    </div>
  </nav>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { UserPlusIcon, ArrowRightOnRectangleIcon, ArrowLeftOnRectangleIcon } from '@heroicons/vue/24/solid'

const router = useRouter()
const auth = ref(!!localStorage.getItem('token'))
const userEmail = ref('User')

function readToken() {
  const token = localStorage.getItem('token')
  if (token) {
    try {
      userEmail.value = atob(token).split(':')[0]
      auth.value = true
    } catch (e) {
      auth.value = false
    }
  } else {
    auth.value = false
  }
}

readToken()

function logout() {
  localStorage.removeItem('token')
  auth.value = false
  router.push('/login')
}
</script>

<style scoped>
.site-nav { position:fixed; top:0; left:0; right:0; z-index:1000; display:flex; align-items:center; justify-content:space-between; padding: .6rem 1rem; border-bottom:1px solid var(--color-border); background:var(--color-background-soft); height:var(--navbar-height, 64px); box-sizing:border-box; box-shadow: 0 2px 8px rgba(2,6,23,0.08); }
.brand { font-weight:700 }
.links { list-style:none; display:flex; gap:1rem; margin:0; padding:0 }
.links a { text-decoration:none; color:var(--color-heading) }
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