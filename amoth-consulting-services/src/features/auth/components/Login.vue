<template>
  <PageWrapper>
    <div class="login">
      <form @submit.prevent="onSubmit" class="login-form" novalidate>
      <h2 class="green">Account Login</h2>

      <div class="field">
        <label for="username"><span class="label-text">Username <span class="required">*</span></span></label>
        <input id="username" v-model="username" type="text" required />
      </div>

      <div class="field">
        <label for="password"><span class="label-text">Password <span class="required">*</span></span></label>
        <input id="password" v-model="password" type="password" @blur="passwordTouched = true" required minlength="6" />
        <p v-if="passwordTouched && password.length < 6" class="error">Password must be at least 6 characters</p>
      </div>

      <button :disabled="!formValid || loading" type="submit" @click="onSubmit">{{ loading ? 'Signing in...' : 'Sign In' }}</button>
      <p v-if="errorMessage" class="error" role="alert">{{ errorMessage }}</p>
      </form>
    </div>
  </PageWrapper>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'

const emit = defineEmits(['login'])
const router = useRouter()

const username = ref('')
const password = ref('')
const loading = ref(false)
const passwordTouched = ref(false)
const errorMessage = ref('')

const formValid = computed(() => username.value && password.value.length >= 6)

function onSubmit() {
  console.log('Login: onSubmit called', { username: username.value, passwordLength: password.value.length })
  passwordTouched.value = true
  if (!formValid.value) return
  loading.value = true
  // mock authentication: persist a token and navigate home
  setTimeout(() => {
    loading.value = false
    const token = btoa(`${username.value}:${password.value}`)
    localStorage.setItem('token', token)
    emit('login', { username: username.value, password: password.value, token })
    // Trigger storage event for other components
    window.dispatchEvent(new Event('authStateChanged'))
    router.push('/dashboard')
  }, 300)
}
</script>

<style scoped>
.login { max-width: 2500px; margin: 2rem auto; padding: 3rem; border: 1px solid var(--color-border); border-radius: 10px; background: var(--color-background-soft); color: var(--color-text); box-shadow: 0 8px 24px rgba(2,6,23,0.08); }
.login-form h2 { margin-bottom: 1.2rem; font-size: 1.4rem; }
.field { margin-bottom: 1rem; display: flex; flex-direction: column }
label { margin-bottom: .25rem; font-weight: 600; color: var(--color-text); }
input { padding: .5rem; border: 1px solid var(--color-border); border-radius: 4px; background: transparent; color: var(--color-text) }
input::placeholder { color: rgba(145,145,145,0.5); }
button { padding: .6rem 1rem; border: none; background: var(--vt-c-indigo); color: #fff; border-radius: 4px; cursor: pointer }
button:hover { transform: translateY(-2px); box-shadow: 0 6px 12px rgba(2,6,23,0.06); transition: all .18s ease }
button[disabled] { opacity: .6; cursor: not-allowed }
.error { color: #e53e3e; font-size: .85rem; margin-top: .25rem }
.required { color: #e53e3e; }
.label-text { display: inline; }
</style>