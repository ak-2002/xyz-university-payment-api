import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';

const Navbar = () => {
  const { user, logout, isAuthenticated, getPrimaryRole } = useAuth();
  const navigate = useNavigate();
  const [isMenuOpen, setIsMenuOpen] = useState(false);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const userRole = getPrimaryRole();
  const isStudent = userRole === 'student';
  const isAdmin = userRole === 'admin';

  return (
    <nav className="bg-white shadow-lg">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between h-16">
          <div className="flex items-center">
            <Link to="/" className="flex-shrink-0 flex items-center">
              <h1 className="text-xl font-bold text-gray-900">XYZ University</h1>
            </Link>
            
            {isAuthenticated && (
              <div className="hidden md:ml-6 md:flex md:space-x-8">
                <Link
                  to="/dashboard"
                  className="text-gray-900 hover:text-gray-500 px-3 py-2 rounded-md text-sm font-medium"
                >
                  Dashboard
                </Link>
                <Link
                  to="/students"
                  className="text-gray-900 hover:text-gray-500 px-3 py-2 rounded-md text-sm font-medium"
                >
                  {isStudent ? 'Profile' : 'Students'}
                </Link>
                <Link
                  to="/payments"
                  className="text-gray-900 hover:text-gray-500 px-3 py-2 rounded-md text-sm font-medium"
                >
                  Payments
                </Link>
                {isAdmin && (
                  <Link
                    to="/fee-management"
                    className="text-gray-900 hover:text-gray-500 px-3 py-2 rounded-md text-sm font-medium"
                  >
                    Fee Management
                  </Link>
                )}
                {!isStudent && (
                  <>
                    <Link
                      to="/reports"
                      className="text-gray-900 hover:text-gray-500 px-3 py-2 rounded-md text-sm font-medium"
                    >
                      Reports
                    </Link>
                    {isAdmin && (
                      <Link
                        to="/role-management"
                        className="text-gray-900 hover:text-gray-500 px-3 py-2 rounded-md text-sm font-medium"
                      >
                        Role Management
                      </Link>
                    )}
                    <Link
                      to="/test-api"
                      className="text-gray-900 hover:text-gray-500 px-3 py-2 rounded-md text-sm font-medium"
                    >
                      Test API
                    </Link>
                  </>
                )}
              </div>
            )}
          </div>

          <div className="flex items-center">
            {isAuthenticated ? (
              <div className="relative">
                <button
                  onClick={() => setIsMenuOpen(!isMenuOpen)}
                  className="flex items-center text-sm rounded-full focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                >
                  <div className="h-8 w-8 rounded-full bg-blue-600 flex items-center justify-center">
                    <span className="text-white font-medium">
                      {user?.name?.charAt(0) || 'U'}
                    </span>
                  </div>
                  <span className="ml-2 text-gray-700">{user?.name || 'User'}</span>
                  <svg className="ml-2 h-4 w-4 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 9l-7 7-7-7" />
                  </svg>
                </button>

                {isMenuOpen && (
                  <div className="absolute right-0 mt-2 w-48 bg-white rounded-md shadow-lg py-1 z-50">
                    <button
                      onClick={handleLogout}
                      className="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                    >
                      Sign out
                    </button>
                  </div>
                )}
              </div>
            ) : (
              <Link
                to="/login"
                className="bg-blue-600 text-white px-4 py-2 rounded-md text-sm font-medium hover:bg-blue-700"
              >
                Sign in
              </Link>
            )}

            {/* Mobile menu button */}
            <div className="md:hidden ml-4">
              <button
                onClick={() => setIsMenuOpen(!isMenuOpen)}
                className="text-gray-400 hover:text-gray-500 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
              >
                <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
                </svg>
              </button>
            </div>
          </div>
        </div>

        {/* Mobile menu */}
        {isMenuOpen && isAuthenticated && (
          <div className="md:hidden">
            <div className="px-2 pt-2 pb-3 space-y-1 sm:px-3">
              <Link
                to="/dashboard"
                className="text-gray-900 hover:text-gray-500 block px-3 py-2 rounded-md text-base font-medium"
                onClick={() => setIsMenuOpen(false)}
              >
                Dashboard
              </Link>
              <Link
                to="/students"
                className="text-gray-900 hover:text-gray-500 block px-3 py-2 rounded-md text-base font-medium"
                onClick={() => setIsMenuOpen(false)}
              >
                {isStudent ? 'Profile' : 'Students'}
              </Link>
              <Link
                to="/payments"
                className="text-gray-900 hover:text-gray-500 block px-3 py-2 rounded-md text-base font-medium"
                onClick={() => setIsMenuOpen(false)}
              >
                Payments
              </Link>
              {isAdmin && (
                <Link
                  to="/fee-management"
                  className="text-gray-900 hover:text-gray-500 block px-3 py-2 rounded-md text-base font-medium"
                  onClick={() => setIsMenuOpen(false)}
                >
                  Fee Management
                </Link>
              )}
              {!isStudent && (
                <>
                  <Link
                    to="/reports"
                    className="text-gray-900 hover:text-gray-500 block px-3 py-2 rounded-md text-base font-medium"
                    onClick={() => setIsMenuOpen(false)}
                  >
                    Reports
                  </Link>
                  {isAdmin && (
                    <Link
                      to="/role-management"
                      className="text-gray-900 hover:text-gray-500 block px-3 py-2 rounded-md text-base font-medium"
                      onClick={() => setIsMenuOpen(false)}
                    >
                      Role Management
                    </Link>
                  )}
                  <Link
                    to="/test-api"
                    className="text-gray-900 hover:text-gray-500 block px-3 py-2 rounded-md text-base font-medium"
                    onClick={() => setIsMenuOpen(false)}
                  >
                    Test API
                  </Link>
                </>
              )}
              <button
                onClick={() => {
                  handleLogout();
                  setIsMenuOpen(false);
                }}
                className="text-gray-900 hover:text-gray-500 block w-full text-left px-3 py-2 rounded-md text-base font-medium"
              >
                Sign out
              </button>
            </div>
          </div>
        )}
      </div>
    </nav>
  );
};

export default Navbar; 