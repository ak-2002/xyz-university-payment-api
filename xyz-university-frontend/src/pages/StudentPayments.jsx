import React, { useState, useEffect } from 'react';
import { useAuth } from '../hooks/useAuth';
import { paymentService } from '../services/paymentService';
import Card from '../components/common/Card';
import LoadingSpinner from '../components/common/LoadingSpinner';
import NotificationModal from '../components/common/NotificationModal';

const StudentPayments = () => {
  const { user } = useAuth();
  const [payments, setPayments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [notification, setNotification] = useState({
    isOpen: false,
    type: 'info',
    title: '',
    message: '',
    details: ''
  });

  useEffect(() => {
    loadStudentPayments();
  }, []);

  const loadStudentPayments = async () => {
    try {
      setLoading(true);
      
      // Get the student number from the user's username (which should be the student number for students)
      // or use a default for demo purposes
      const studentNumber = user?.name || 'S66001'; // Default to Alex for demo
      console.log('Loading payments for student:', studentNumber);
      
      // Use the specific endpoint for student payments
      const response = await paymentService.getPaymentsByStudent(studentNumber);
      console.log('Student payments response:', response);
      
      // Extract the payments data from the response
      const studentPayments = response.data || [];
      console.log('Student payments data:', studentPayments);
      
      setPayments(studentPayments);
    } catch (err) {
      console.error('Error loading student payments:', err);
      setError('Failed to load payment history');
    } finally {
      setLoading(false);
    }
  };

  const showNotification = (type, title, message, details = '') => {
    setNotification({
      isOpen: true,
      type,
      title,
      message,
      details
    });
  };

  const closeNotification = () => {
    setNotification(prev => ({ ...prev, isOpen: false }));
  };

  const downloadReceipt = (payment) => {
    // Create receipt content
    const receiptContent = `
XYZ UNIVERSITY PAYMENT RECEIPT
================================

Receipt Number: ${payment.paymentReference}
Date: ${new Date(payment.paymentDate).toLocaleDateString()}
Student: ${payment.studentName || 'Student'}
Student Number: ${payment.studentNumber}
Amount Paid: $${payment.amountPaid?.toFixed(2)}
Payment Method: ${payment.paymentMethod || 'M-Pesa'}

Thank you for your payment!

Generated on: ${new Date().toLocaleString()}
    `;

    // Create and download file
    const blob = new Blob([receiptContent], { type: 'text/plain' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `receipt-${payment.paymentReference}.txt`;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);

    showNotification('success', 'Receipt Downloaded', 'Payment receipt has been downloaded successfully.');
  };

  const totalPaid = payments.reduce((sum, payment) => sum + (payment.amountPaid || 0), 0);
  const outstandingBalance = 5000 - totalPaid; // Assuming total tuition is 5000

  if (loading) {
    return <LoadingSpinner size="large" text="Loading payment history..." />;
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 via-blue-50/30 to-purple-50/30 p-6 space-y-8">
      {/* Header */}
      <Card variant="elevated" className="p-8">
        <div className="text-center">
          <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent mb-3">
            My Payments
          </h1>
          <p className="text-lg text-gray-600 mb-6">
            Manage your payments and view payment history
          </p>
        </div>
      </Card>

      {/* Financial Summary */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
        <Card variant="elevated" className="p-6 group">
          <div className="flex items-center">
            <div className="p-4 rounded-2xl bg-gradient-to-br from-red-100 to-red-200 text-red-600 group-hover:scale-110 transition-transform duration-300">
              <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1" />
              </svg>
            </div>
            <div className="ml-6">
              <p className="text-sm font-medium text-gray-600 mb-1">Outstanding Balance</p>
              <p className="text-3xl font-bold text-gray-900">${outstandingBalance.toLocaleString()}</p>
            </div>
          </div>
        </Card>

        <Card variant="elevated" className="p-6 group">
          <div className="flex items-center">
            <div className="p-4 rounded-2xl bg-gradient-to-br from-green-100 to-green-200 text-green-600 group-hover:scale-110 transition-transform duration-300">
              <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <div className="ml-6">
              <p className="text-sm font-medium text-gray-600 mb-1">Total Paid</p>
              <p className="text-3xl font-bold text-gray-900">${totalPaid.toLocaleString()}</p>
            </div>
          </div>
        </Card>

        <Card variant="elevated" className="p-6 group">
          <div className="flex items-center">
            <div className="p-4 rounded-2xl bg-gradient-to-br from-blue-100 to-blue-200 text-blue-600 group-hover:scale-110 transition-transform duration-300">
              <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
            </div>
            <div className="ml-6">
              <p className="text-sm font-medium text-gray-600 mb-1">Payment History</p>
              <p className="text-3xl font-bold text-gray-900">{payments.length}</p>
            </div>
          </div>
        </Card>
      </div>

      {/* Action Buttons */}
      <Card variant="elevated" className="p-6">
        <div className="flex flex-col sm:flex-row gap-4">
          <button
            onClick={() => window.print()}
            className="px-6 py-3 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition-colors font-medium"
          >
            Print Statement
          </button>
        </div>
      </Card>

      {/* Payment History */}
      <Card variant="elevated" className="p-6">
        <h3 className="text-xl font-bold text-gray-900 mb-6 flex items-center">
          <span className="w-2 h-8 bg-gradient-to-b from-purple-500 to-pink-500 rounded-full mr-3"></span>
          Payment History
        </h3>
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gradient-to-r from-gray-50 to-gray-100/50">
              <tr>
                <th className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">
                  Date
                </th>
                <th className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">
                  Reference
                </th>
                <th className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">
                  Amount
                </th>
                <th className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">
                  Method
                </th>
                <th className="px-6 py-4 text-left text-xs font-bold text-gray-500 uppercase tracking-wider">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-100">
              {payments.length > 0 ? (
                payments.map((payment) => (
                  <tr key={payment.id} className="hover:bg-gray-50/50 transition-colors duration-200">
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                      {new Date(payment.paymentDate).toLocaleDateString()}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {payment.paymentReference}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-bold text-gray-900">
                      ${payment.amountPaid?.toFixed(2)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className={`px-3 py-1 text-xs font-semibold rounded-full ${
                        payment.paymentMethod === 'M-Pesa' ? 'bg-green-100 text-green-800' :
                        payment.paymentMethod === 'Cash' ? 'bg-blue-100 text-blue-800' :
                        payment.paymentMethod === 'Cheque' ? 'bg-purple-100 text-purple-800' :
                        'bg-gray-100 text-gray-800'
                      }`}>
                        {payment.paymentMethod || 'M-Pesa'}
                      </span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                      <button
                        onClick={() => downloadReceipt(payment)}
                        className="text-blue-600 hover:text-blue-900"
                      >
                        Download Receipt
                      </button>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan="5" className="px-6 py-8 text-center text-gray-500">
                    <div className="flex flex-col items-center">
                      <svg className="w-12 h-12 text-gray-300 mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                      </svg>
                      <p className="text-sm">No payment history found</p>
                      <p className="text-xs text-gray-400 mt-1">Your payment history will appear here</p>
                    </div>
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </Card>

      {/* Notification Modal */}
      <NotificationModal
        isOpen={notification.isOpen}
        onClose={closeNotification}
        type={notification.type}
        title={notification.title}
        message={notification.message}
        details={notification.details}
      />
    </div>
  );
};

export default StudentPayments; 