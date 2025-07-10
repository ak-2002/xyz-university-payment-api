import api from './api';

const API_BASE_URL = 'http://localhost:5260';

export const authService = {
  // Login with username and password using the main API
  async login(username, password) {
    try {
      const response = await fetch(`${API_BASE_URL}/api/Authorization/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          username: username,
          password: password
        }),
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Login failed');
      }

      const data = await response.json();
      
      if (data.success && data.data) {
        // Extract role names from RoleDto objects
        const roleNames = data.data.user?.roles?.map(role => role.name) || [];
        
        // Create user object with role names
        const userWithRoles = {
          ...data.data.user,
          roles: roleNames
        };
        
        // Store tokens with consistent keys
        localStorage.setItem('authToken', data.data.token);
        localStorage.setItem('refreshToken', data.data.refreshToken);
        localStorage.setItem('user', JSON.stringify(userWithRoles));
        localStorage.setItem('expires_in', data.data.expiresAt);
        localStorage.setItem('token_type', 'Bearer');
        
        // Store user roles as array of role names
        localStorage.setItem('user_roles', JSON.stringify(roleNames));
        
        // Return data with updated user object
        return {
          ...data.data,
          user: userWithRoles
        };
      } else {
        throw new Error(data.message || 'Login failed');
      }
    } catch (error) {
      console.error('Login error:', error);
      throw error;
    }
  },

  // Logout user
  logout() {
    localStorage.removeItem('authToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    localStorage.removeItem('expires_in');
    localStorage.removeItem('token_type');
    localStorage.removeItem('user_roles');
  },

  // Check if user is authenticated
  isAuthenticated() {
    const token = localStorage.getItem('authToken');
    return !!token;
  },

  // Get current user info
  getCurrentUser() {
    const user = localStorage.getItem('user');
    const roles = localStorage.getItem('user_roles');
    
    if (user) {
      const userData = JSON.parse(user);
      if (roles) {
        userData.roles = JSON.parse(roles);
      }
      return userData;
    }
    
    return null;
  },

  // Set user info
  setCurrentUser(user) {
    localStorage.setItem('user', JSON.stringify(user));
    if (user.roles) {
      localStorage.setItem('user_roles', JSON.stringify(user.roles));
    }
  },

  // Get user roles
  getUserRoles() {
    const roles = localStorage.getItem('user_roles');
    return roles ? JSON.parse(roles) : [];
  },

  // Refresh token
  async refreshToken() {
    try {
      const refreshToken = localStorage.getItem('refreshToken');
      if (!refreshToken) {
        throw new Error('No refresh token available');
      }

      const response = await fetch(`${API_BASE_URL}/api/Authorization/refresh-token`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          refreshToken: refreshToken
        }),
      });

      if (!response.ok) {
        throw new Error('Token refresh failed');
      }

      const data = await response.json();
      
      if (data.success && data.data) {
        // Extract role names from RoleDto objects
        const roleNames = data.data.user?.roles?.map(role => role.name) || [];
        
        // Create user object with role names
        const userWithRoles = {
          ...data.data.user,
          roles: roleNames
        };
        
        // Update tokens with consistent keys
        localStorage.setItem('authToken', data.data.token);
        localStorage.setItem('refreshToken', data.data.refreshToken);
        localStorage.setItem('user', JSON.stringify(userWithRoles));
        localStorage.setItem('expires_in', data.data.expiresAt);
        
        // Update user roles as array of role names
        localStorage.setItem('user_roles', JSON.stringify(roleNames));
        
        // Return data with updated user object
        return {
          ...data.data,
          user: userWithRoles
        };
      } else {
        throw new Error(data.message || 'Token refresh failed');
      }
    } catch (error) {
      console.error('Token refresh error:', error);
      this.logout();
      throw error;
    }
  },
}; 