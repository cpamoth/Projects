 // src/stores/auth.js
    import { defineStore } from 'pinia';
    import { ref } from 'vue';

    export const useAuthStore = defineStore('auth', () => {
        const user = ref(null);
        const isAuthenticated = ref(false);
        const error = ref(null);

        async function login(username, password) {
            try {
                // Simulate an API call for login
                const response = await new Promise(resolve => setTimeout(() => {
                    if (username === 'user' && password === 'password') {
                        resolve({ success: true, user: { id: 1, username: 'user' } });
                    } else {
                        resolve({ success: false, message: 'Invalid credentials' });
                    }
                }, 1000));

                if (response.success) {
                    user.value = response.user;
                    isAuthenticated.value = true;
                    error.value = null;
                } else {
                    error.value = response.message;
                    isAuthenticated.value = false;
                    user.value = null;
                }
            } catch (err) {
                error.value = 'An error occurred during login.';
                isAuthenticated.value = false;
                user.value = null;
            }
        }

        function logout() {
            user.value = null;
            isAuthenticated.value = false;
            error.value = null;
        }

        return { user, isAuthenticated, error, login, logout };
    });