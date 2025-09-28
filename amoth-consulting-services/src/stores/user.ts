import { defineStore } from 'pinia';

// Define the User type (customize fields as needed)
export interface User {
  id: number;
  name: string;
  email: string;
}

export const useUserStore = defineStore('user', {
  state: () => ({
    isAuthenticated: false,
    user: null as User | null,
  }),
  actions: {
    login(userData: User) {
      this.isAuthenticated = true;
      this.user = userData;
    },
    logout() {
      this.isAuthenticated = false;
      this.user = null;
    },
  },
});