import { useState, useEffect, createContext, useContext } from 'react';
import { authService } from '../services/authService';

// Create auth context
const AuthContext = createContext();

// Custom hook to use auth context
export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

// Auth provider component
export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Check authentication status on mount
  useEffect(() => {
    const checkAuth = () => {
      try {
        const isAuthenticated = authService.isAuthenticated();
        const currentUser = authService.getCurrentUser();
        
        if (isAuthenticated && currentUser) {
          setUser(currentUser);
        } else if (isAuthenticated) {
          // Token exists but no user data, try to get user info
          // This could be expanded to fetch user info from an API endpoint
          setUser({ id: 'user', name: 'User' });
        }
      } catch (error) {
        console.error('Auth check error:', error);
        authService.logout();
      } finally {
        setLoading(false);
      }
    };

    checkAuth();
  }, []);

  // Login function
  const login = async (username, password) => {
    try {
      setLoading(true);
      setError(null);
      
      const authData = await authService.login(username, password);
      
      // Set user data with roles
      const userData = {
        id: authData.user?.id || 'user',
        name: authData.user?.name || username,
        email: authData.user?.email,
        roles: authData.user?.roles || []
      };
      
      authService.setCurrentUser(userData);
      setUser(userData);
      
      return authData;
    } catch (error) {
      setError(error.message || 'Login failed');
      throw error;
    } finally {
      setLoading(false);
    }
  };

  // Logout function
  const logout = () => {
    authService.logout();
    setUser(null);
    setError(null);
  };

  // Refresh token function
  const refreshToken = async () => {
    try {
      const authData = await authService.refreshToken();
      return authData;
    } catch (error) {
      logout();
      throw error;
    }
  };

  // Check if user has specific role
  const hasRole = (role) => {
    if (!user || !user.roles) return false;
    return user.roles.includes(role);
  };

  // Check if user has any of the specified roles
  const hasAnyRole = (roles) => {
    if (!user || !user.roles) return false;
    return roles.some(role => user.roles.includes(role));
  };

  // Get user's primary role for routing
  const getPrimaryRole = () => {
    if (!user || !user.roles) return 'student';
    
    // Priority order: Admin > Manager > Staff > Student
    if (user.roles.includes('Admin')) return 'admin';
    if (user.roles.includes('Manager')) return 'manager';
    if (user.roles.includes('Staff')) return 'staff';
    
    return 'student';
  };

  const value = {
    user,
    loading,
    error,
    login,
    logout,
    refreshToken,
    isAuthenticated: !!user,
    hasRole,
    hasAnyRole,
    getPrimaryRole,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}; 