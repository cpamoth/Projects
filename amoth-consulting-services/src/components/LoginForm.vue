 <!-- src/components/LoginForm.vue -->
    <template>
        <div>
            <h2>Login</h2>
            <form @submit.prevent="handleLogin">
                <div>
                    <label for="username">Username:</label>
                    <input type="text" id="username" v-model="username" required />
                </div>
                <div>
                    <label for="password">Password:</label>
                    <input type="password" id="password" v-model="password" required />
                </div>
                <button type="submit">Login</button>
                <p v-if="authStore.error" class="error">{{ authStore.error }}</p>
            </form>
        </div>
    </template>

    <script setup>
    import { ref } from 'vue';
    import { useAuthStore } from '../stores/auth';

    const authStore = useAuthStore();
    const username = ref('');
    const password = ref('');

    const handleLogin = async () => {
        await authStore.login(username.value, password.value);
        if (authStore.isAuthenticated) {
            // Redirect or perform other actions on successful login
            console.log('Login successful!');
        }
    };
    </script>

    <style scoped>
    .error {
        color: red;
    }
    </style>
