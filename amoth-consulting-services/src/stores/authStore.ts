import { defineStore } from 'pinia';

export const useAuthStore = defineStore('auth', {
  state: () => ({
    isAuthenticated: false,
    user: { username: '' }
  }),
  actions: {
    login(username: string) {
      this.isAuthenticated = true;
      this.user.username = username;
    },
    logout() {
      this.isAuthenticated = false;
      this.user.username = '';
    }
  }
});