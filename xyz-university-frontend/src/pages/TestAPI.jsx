import React, { useState } from 'react';

const TestAPI = () => {
  const [results, setResults] = useState({});
  const [loading, setLoading] = useState(false);
  const [authToken, setAuthToken] = useState('');
  const [loginData, setLoginData] = useState({
    username: 'admin',
    password: 'Admin123!'
  });

  const baseUrl = 'http://localhost:5260';

  const testEndpoint = async (name, url, options = {}) => {
    try {
      const response = await fetch(url, {
        method: options.method || 'GET',
        headers: {
          'Content-Type': 'application/json',
          ...(authToken && { 'Authorization': `Bearer ${authToken}` }),
          ...options.headers
        },
        body: options.body ? JSON.stringify(options.body) : undefined,
        ...options
      });

      const data = await response.text();
      let parsedData;
      try {
        parsedData = JSON.parse(data);
      } catch {
        parsedData = data;
      }

      return {
        success: response.ok,
        status: response.status,
        data: parsedData
      };
    } catch (error) {
      return {
        success: false,
        error: error.message
      };
    }
  };

  // Special login function that uses the exact same format as the working authService
  const testLogin = async (name, url, options = {}) => {
    try {
      const response = await fetch(url, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          username: options.body.username,
          password: options.body.password
        }),
      });

      if (!response.ok) {
        const errorData = await response.json();
        return {
          success: false,
          status: response.status,
          error: errorData.message || 'Login failed',
          data: errorData
        };
      }

      const data = await response.json();
      
      return {
        success: true,
        status: response.status,
        data: data
      };
    } catch (error) {
      return {
        success: false,
        error: error.message
      };
    }
  };

  const runAllTests = async () => {
    setLoading(true);
    const newResults = {};

    // Test 1: Base API connectivity
    console.log('Testing base API...');
    newResults['Base API'] = await testEndpoint('Base API', `${baseUrl}/`);

    // Test 2: Swagger documentation
    console.log('Testing Swagger...');
    newResults['Swagger'] = await testEndpoint('Swagger', `${baseUrl}/swagger/index.html`);

    // Test 3: Public authentication endpoints
    console.log('Testing public auth endpoints...');
    newResults['Auth - Test JWT Config'] = await testEndpoint('Auth - Test JWT Config', `${baseUrl}/api/Authorization/test-jwt-config`);
    newResults['Auth - Check Database State'] = await testEndpoint('Auth - Check Database State', `${baseUrl}/api/Authorization/check-database-state`);
    newResults['Auth - Check Roles'] = await testEndpoint('Auth - Check Roles', `${baseUrl}/api/Authorization/check-roles`);

    // Test 4: Login to get token
    console.log('Testing login...');
    const loginResult = await testLogin('Login', `${baseUrl}/api/Authorization/login`, {
      body: {
        username: loginData.username,
        password: loginData.password
      }
    });
    newResults['Login'] = loginResult;

    // If login successful, extract token and test authenticated endpoints
    if (loginResult.success && loginResult.data?.data?.token) {
      const token = loginResult.data.data.token;
      setAuthToken(token);
      console.log('Login successful, testing authenticated endpoints...');

      // Test authenticated endpoints with token
      newResults['V3 Students (Authenticated)'] = await testEndpoint('V3 Students (Authenticated)', `${baseUrl}/api/v3/StudentControllerV`, {
        headers: { 'Authorization': `Bearer ${token}` }
      });

      newResults['V3 Payments (Authenticated)'] = await testEndpoint('V3 Payments (Authenticated)', `${baseUrl}/api/v3/Payment`, {
        headers: { 'Authorization': `Bearer ${token}` }
      });

      newResults['V2 Students (Authenticated)'] = await testEndpoint('V2 Students (Authenticated)', `${baseUrl}/api/v2/students`, {
        headers: { 'Authorization': `Bearer ${token}` }
      });

      newResults['V2 Payments (Authenticated)'] = await testEndpoint('V2 Payments (Authenticated)', `${baseUrl}/api/v2/payments`, {
        headers: { 'Authorization': `Bearer ${token}` }
      });

      // Test user info endpoint
      newResults['My Info (Authenticated)'] = await testEndpoint('My Info (Authenticated)', `${baseUrl}/api/Authorization/my-info`, {
        headers: { 'Authorization': `Bearer ${token}` }
      });

      // Test dashboard endpoint
      newResults['V3 Dashboard Student Stats (Authenticated)'] = await testEndpoint('V3 Dashboard Student Stats (Authenticated)', `${baseUrl}/api/v3/Dashboard/student-stats`, {
        headers: { 'Authorization': `Bearer ${token}` }
      });

      // Test dashboard test endpoint (no auth required)
      newResults['V3 Dashboard Test (No Auth)'] = await testEndpoint('V3 Dashboard Test (No Auth)', `${baseUrl}/api/v3/Dashboard/test`);
    } else {
      console.log('Login failed, skipping authenticated endpoint tests');
    }

    // Test 5: Test endpoints without authentication (should return 401, not 404)
    console.log('Testing endpoints without authentication...');
    newResults['V3 Students (No Auth)'] = await testEndpoint('V3 Students (No Auth)', `${baseUrl}/api/v3/StudentControllerV`);
    newResults['V3 Payments (No Auth)'] = await testEndpoint('V3 Payments (No Auth)', `${baseUrl}/api/v3/Payment`);
    newResults['V2 Students (No Auth)'] = await testEndpoint('V2 Students (No Auth)', `${baseUrl}/api/v2/students`);
    newResults['V2 Payments (No Auth)'] = await testEndpoint('V2 Payments (No Auth)', `${baseUrl}/api/v2/payments`);

    setResults(newResults);
    setLoading(false);
  };

  const retestEndpoint = async (name, url, options = {}) => {
    const result = await testEndpoint(name, url, options);
    setResults(prev => ({ ...prev, [name]: result }));
  };

  const handleLogin = async () => {
    const result = await testLogin('Login', `${baseUrl}/api/Authorization/login`, {
      body: {
        username: loginData.username,
        password: loginData.password
      }
    });
    
    console.log('Login result:', result); // Debug log
    
    if (result.success && result.data?.success && result.data?.data?.token) {
      setAuthToken(result.data.data.token);
      alert('Login successful! Token saved for testing.');
    } else {
      const errorMessage = result.data?.message || result.data?.Message || result.error || 'Login failed';
      alert('Login failed: ' + errorMessage);
    }
  };

  const getStatusIcon = (result) => {
    if (!result) return '⏳';
    if (result.success) return '✅';
    if (result.status === 401) return '🔒';
    if (result.status === 404) return '❌';
    return '⚠️';
  };

  const getStatusText = (result) => {
    if (!result) return 'Not tested';
    if (result.success) return 'Success';
    if (result.status === 401) return 'Unauthorized (Expected)';
    if (result.status === 404) return 'Not Found';
    return `Error: ${result.error || result.status}`;
  };

  return (
    <div className="p-6 max-w-6xl mx-auto">
      <h1 className="text-3xl font-bold mb-6">API Connectivity Test</h1>
      
      {/* Login Section */}
      <div className="bg-blue-50 p-4 rounded-lg mb-6">
        <h2 className="text-xl font-semibold mb-4">Authentication Setup</h2>
        <div className="flex gap-4 items-end">
          <div>
            <label className="block text-sm font-medium mb-1">Username:</label>
            <input
              type="text"
              value={loginData.username}
              onChange={(e) => setLoginData(prev => ({ ...prev, username: e.target.value }))}
              className="border rounded px-3 py-2 w-40"
            />
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">Password:</label>
            <input
              type="password"
              value={loginData.password}
              onChange={(e) => setLoginData(prev => ({ ...prev, password: e.target.value }))}
              className="border rounded px-3 py-2 w-40"
            />
          </div>
          <button
            onClick={handleLogin}
            className="bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600"
          >
            Login
          </button>
        </div>
        {authToken && (
          <div className="mt-2 text-sm text-green-600">
            ✅ Token available for testing
          </div>
        )}
      </div>

      {/* Test Controls */}
      <div className="mb-6">
        <button
          onClick={runAllTests}
          disabled={loading}
          className="bg-green-500 text-white px-6 py-3 rounded-lg hover:bg-green-600 disabled:bg-gray-400"
        >
          {loading ? 'Running Tests...' : 'Run All API Tests'}
        </button>
      </div>

      {/* Results */}
      <div className="space-y-4">
        <h2 className="text-2xl font-semibold">Test Results</h2>
        
        {/* Public Endpoints */}
        <div className="bg-gray-50 p-4 rounded-lg">
          <h3 className="text-lg font-semibold mb-3">Public Endpoints</h3>
          <div className="space-y-2">
            {['Base API', 'Swagger', 'Auth - Test JWT Config', 'Auth - Check Database State', 'Auth - Check Roles'].map(name => (
              <div key={name} className="flex items-center justify-between p-2 bg-white rounded">
                <span className="font-medium">{name}</span>
                <div className="flex items-center gap-2">
                  <span className={`px-2 py-1 rounded text-sm ${
                    !results[name] ? 'bg-gray-200' :
                    results[name].success ? 'bg-green-200 text-green-800' :
                    results[name].status === 401 ? 'bg-yellow-200 text-yellow-800' :
                    'bg-red-200 text-red-800'
                  }`}>
                    {getStatusIcon(results[name])} {getStatusText(results[name])}
                  </span>
                  {results[name] && (
                                       <button
                     onClick={() => {
                       const url = name === 'Base API' ? `${baseUrl}/` :
                                  name === 'Swagger' ? `${baseUrl}/swagger/index.html` :
                                  `${baseUrl}/api/Authorization/${name.toLowerCase().replace('auth - ', '').replace(/ /g, '-')}`;
                       retestEndpoint(name, url);
                     }}
                      className="text-blue-500 hover:text-blue-700 text-sm"
                    >
                      Retest
                    </button>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Authentication Test */}
        <div className="bg-gray-50 p-4 rounded-lg">
          <h3 className="text-lg font-semibold mb-3">Authentication</h3>
          <div className="flex items-center justify-between p-2 bg-white rounded">
            <span className="font-medium">Login</span>
            <div className="flex items-center gap-2">
              <span className={`px-2 py-1 rounded text-sm ${
                !results['Login'] ? 'bg-gray-200' :
                results['Login'].success ? 'bg-green-200 text-green-800' :
                'bg-red-200 text-red-800'
              }`}>
                {getStatusIcon(results['Login'])} {getStatusText(results['Login'])}
              </span>
                             {results['Login'] && (
                 <button
                   onClick={() => {
                     testLogin('Login', `${baseUrl}/api/Authorization/login`, {
                       body: {
                         username: loginData.username,
                         password: loginData.password
                       }
                     }).then(result => {
                       setResults(prev => ({ ...prev, 'Login': result }));
                     });
                   }}
                   className="text-blue-500 hover:text-blue-700 text-sm"
                 >
                   Retest
                 </button>
               )}
            </div>
          </div>
        </div>

        {/* Authenticated Endpoints */}
        <div className="bg-gray-50 p-4 rounded-lg">
          <h3 className="text-lg font-semibold mb-3">Authenticated Endpoints</h3>
          <div className="space-y-2">
            {['V3 Students (Authenticated)', 'V3 Payments (Authenticated)', 'V2 Students (Authenticated)', 'V2 Payments (Authenticated)', 'My Info (Authenticated)', 'V3 Dashboard Student Stats (Authenticated)', 'V3 Dashboard Test (No Auth)'].map(name => (
              <div key={name} className="flex items-center justify-between p-2 bg-white rounded">
                <span className="font-medium">{name}</span>
                <div className="flex items-center gap-2">
                  <span className={`px-2 py-1 rounded text-sm ${
                    !results[name] ? 'bg-gray-200' :
                    results[name].success ? 'bg-green-200 text-green-800' :
                    results[name].status === 401 ? 'bg-yellow-200 text-yellow-800' :
                    'bg-red-200 text-red-800'
                  }`}>
                    {getStatusIcon(results[name])} {getStatusText(results[name])}
                  </span>
                  {results[name] && authToken && (
                    <button
                      onClick={() => {
                                                 const url = name.includes('V3 Students') ? `${baseUrl}/api/v3/students` :
                                    name.includes('V3 Payments') ? `${baseUrl}/api/v3/payments` :
                                    name.includes('V2 Students') ? `${baseUrl}/api/v2/students` :
                                    name.includes('V2 Payments') ? `${baseUrl}/api/v2/payments` :
                                    name.includes('V3 Dashboard') ? `${baseUrl}/api/v3/Dashboard/student-stats` :
                                    `${baseUrl}/api/Authorization/my-info`;
                        retestEndpoint(name, url, {
                          headers: { 'Authorization': `Bearer ${authToken}` }
                        });
                      }}
                      className="text-blue-500 hover:text-blue-700 text-sm"
                    >
                      Retest
                    </button>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Unauthenticated Endpoints (Should Fail) */}
        <div className="bg-gray-50 p-4 rounded-lg">
          <h3 className="text-lg font-semibold mb-3">Unauthenticated Endpoints (Expected to Fail)</h3>
          <div className="space-y-2">
            {['V3 Students (No Auth)', 'V3 Payments (No Auth)', 'V2 Students (No Auth)', 'V2 Payments (No Auth)'].map(name => (
              <div key={name} className="flex items-center justify-between p-2 bg-white rounded">
                <span className="font-medium">{name}</span>
                <div className="flex items-center gap-2">
                  <span className={`px-2 py-1 rounded text-sm ${
                    !results[name] ? 'bg-gray-200' :
                    results[name].status === 401 ? 'bg-green-200 text-green-800' : // 401 is expected
                    results[name].success ? 'bg-yellow-200 text-yellow-800' : // Success is unexpected
                    'bg-red-200 text-red-800'
                  }`}>
                    {getStatusIcon(results[name])} {getStatusText(results[name])}
                  </span>
                  {results[name] && (
                    <button
                      onClick={() => {
                        const url = name.includes('V3 Students') ? `${baseUrl}/api/v3/students` :
                                   name.includes('V3 Payments') ? `${baseUrl}/api/v3/payments` :
                                   name.includes('V2 Students') ? `${baseUrl}/api/v2/students` :
                                   `${baseUrl}/api/v2/payments`;
                        retestEndpoint(name, url);
                      }}
                      className="text-blue-500 hover:text-blue-700 text-sm"
                    >
                      Retest
                    </button>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Detailed Results */}
      {Object.keys(results).length > 0 && (
        <div className="mt-8">
          <h2 className="text-2xl font-semibold mb-4">Detailed Results</h2>
          <div className="space-y-4">
            {Object.entries(results).map(([name, result]) => (
              <div key={name} className="border rounded-lg p-4">
                <h3 className="font-semibold mb-2">{name}</h3>
                <pre className="bg-gray-100 p-3 rounded text-sm overflow-auto">
                  {JSON.stringify(result, null, 2)}
                </pre>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};

export default TestAPI; 