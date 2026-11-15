<template>
  <section class="reset-password" aria-labelledby="reset-heading">
    <h2 id="reset-heading" class="green">{{ token ? 'Choose a new password' : 'Reset your password' }}</h2>

    <form v-if="!token && !requestSent" @submit.prevent="submitRequest" class="reset-form">
      <p>Enter the email address associated with your account and we’ll send a reset link.</p>
      <label for="reset-email">Email</label>
      <input id="reset-email" v-model="email" type="email" maxlength="254" autocomplete="email" required />
      <button type="submit" :disabled="loading">{{ loading ? 'Sending…' : 'Send reset link' }}</button>
    </form>

    <form v-else-if="token && !resetComplete" @submit.prevent="submitReset" class="reset-form">
      <label for="new-password">New password</label>
      <input id="new-password" v-model="password" type="password" minlength="12" maxlength="128" autocomplete="new-password" required />
      <p class="hint">Use at least 12 characters.</p>
      <label for="confirm-password">Confirm new password</label>
      <input id="confirm-password" v-model="confirmation" type="password" minlength="12" maxlength="128" autocomplete="new-password" required />
      <button type="submit" :disabled="loading">{{ loading ? 'Updating…' : 'Reset password' }}</button>
    </form>

    <p v-if="requestSent" class="success" role="status">
      If an account exists for that email, reset instructions will be sent. Check your spam folder or contact support if no email arrives.
    </p>
    <p v-if="resetComplete" class="success" role="status">Your password has been reset. You can now sign in.</p>
    <p v-if="errorMessage" class="error" role="alert">{{ errorMessage }}</p>
    <button v-if="requestSent || resetComplete" type="button" class="back-button" @click="emit('close')">Back to sign in</button>
    <button v-else type="button" class="back-button" @click="emit('close')">Cancel</button>
  </section>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { requestPasswordReset, resetPassword } from '../../../services/authApi.js'

const router = useRouter()
const props = defineProps({
  token: {
    type: String,
    default: '',
  },
})
const emit = defineEmits(['close'])

const token = props.token
const email = ref('')
const password = ref('')
const confirmation = ref('')
const loading = ref(false)
const requestSent = ref(false)
const resetComplete = ref(false)
const errorMessage = ref('')

async function submitRequest() {
  loading.value = true
  errorMessage.value = ''
  try {
    await requestPasswordReset(email.value.trim())
    requestSent.value = true
  } catch (error) {
    errorMessage.value = error.message
  } finally {
    loading.value = false
  }
}

async function submitReset() {
  errorMessage.value = ''
  if (password.value !== confirmation.value) {
    errorMessage.value = 'Passwords do not match.'
    return
  }
  if (password.value.length < 12) {
    errorMessage.value = 'Password must be at least 12 characters.'
    return
  }

  loading.value = true
  try {
    await resetPassword(token, password.value)
    resetComplete.value = true
    router.replace('/login')
  } catch (error) {
    errorMessage.value = error.message
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.reset-password { max-width: 32rem; margin: 2rem auto; padding: 2rem; border: 1px solid var(--color-border); border-radius: 10px; background: var(--color-background-soft); color: var(--color-text); }
.reset-password h2 { margin-bottom: 1rem; }
.reset-form { display: flex; flex-direction: column; gap: .75rem; }
label { font-weight: 600; }
input { padding: .6rem; border: 1px solid var(--color-border); border-radius: 4px; background: transparent; color: var(--color-text); }
button { align-self: flex-start; padding: .6rem 1rem; border: none; border-radius: 4px; background: var(--vt-c-indigo); color: #fff; cursor: pointer; }
button:disabled { opacity: .6; cursor: not-allowed; }
.back-button { margin-top: 1rem; }
.hint { margin: -.5rem 0 0; font-size: .85rem; opacity: .8; }
.error { color: #e53e3e; }
.success { color: #2f855a; }
</style>
