<template>
  <PageWrapper>
    <div class="register">
      <form @submit.prevent="onSubmit" class="register-form">
      <h2 class="green">Sign Up for New Account</h2>

      <label for="fullname">
        <span class="label-text">Full Name <span class="required">*</span></span>
        <input id="fullname" v-model="fullName" type="text" required />
      </label>

      <label for="username">
        <span class="label-text">Username <span class="required">*</span></span>
        <input id="username" v-model="username" type="text" required />
      </label>

      <label for="password">
        <span class="label-text">Password <span class="required">*</span></span>
        <input id="password" v-model="password" type="password" minlength="6" required />
      </label>

      <label for="confirm">
        <span class="label-text">Confirm Password <span class="required">*</span></span>
        <input id="confirm" v-model="confirm" type="password" minlength="6" required />
      </label>

      <button :disabled="loading || !canSubmit" type="submit">{{ loading ? 'Creating…' : 'Register' }}</button>
      <p v-if="error" class="error" role="alert">{{ error }}</p>
      </form>
    </div>
  </PageWrapper>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const fullName = ref('')
const username = ref('')
const email = ref('')
const password = ref('')
const confirm = ref('')
const loading = ref(false)
const error = ref('')

const canSubmit = computed(() => fullName.value.trim() && username.value && password.value.length >= 6)

function onSubmit() {
  error.value = ''
  if (password.value !== confirm.value) {
    error.value = 'Passwords do not match'
    return
  }
  loading.value = true
  setTimeout(() => {
    loading.value = false
    const token = btoa(`${username.value}:${password.value}`)
    localStorage.setItem('token', token)
    localStorage.setItem('userProfile', JSON.stringify({ fullName: fullName.value, username: username.value, email: email.value }))
    // Trigger auth state change event
    window.dispatchEvent(new Event('authStateChanged'))
    router.push('/dashboard')
  }, 600)
}
</script>

<style scoped>
.register { max-width: 500px; margin: 2rem auto; padding: 2rem; border: 1px solid var(--color-border); border-radius: 8px; background: var(--color-background-soft); color: var(--color-text); box-shadow: 0 6px 18px rgba(2,6,23,0.04); }
.register-form { display:flex; flex-direction:column; gap:.6rem }
.register-form h2 { margin-bottom: 1rem; font-size: 1.25rem; }
label { margin-bottom: .25rem; font-weight: 600; color: var(--color-text); display:flex; flex-direction:column; text-align: left }
input { padding: .5rem; border: 1px solid var(--color-border); border-radius: 4px; background: transparent; color: var(--color-text) }
input::placeholder { color: rgba(145,145,145,0.5); }
button { padding: .6rem 1rem; border: none; background: var(--vt-c-indigo); color: #fff; border-radius: 4px; cursor: pointer }
button:hover { transform: translateY(-2px); box-shadow: 0 6px 12px rgba(2,6,23,0.06); transition: all .18s ease }
button[disabled] { opacity: .6; cursor: not-allowed }
.error { color: #e53e3e; font-size: .85rem; margin-top: .25rem }
.required { color: #e53e3e; }
.label-text { display: block; margin-bottom: .5rem; }
</style>
