import React, { useState, useEffect } from 'react';
import Card from '../common/Card';
import Button from '../common/Button';
import Alert from '../common/ErrorMessage';
import Spinner from '../common/LoadingSpinner';
import { useAuth } from '../../hooks/useAuth';
import api from '../../services/api';

const StudentFeeBalance = ({ studentNumber }) => {
    const { user } = useAuth();
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [balanceSummary, setBalanceSummary] = useState(null);
    const [paymentAmount, setPaymentAmount] = useState('');
    const [selectedBalanceId, setSelectedBalanceId] = useState(null);

    useEffect(() => {
        if (studentNumber) {
            loadStudentBalance();
        }
    }, [studentNumber]);

    const loadStudentBalance = async () => {
        try {
            setLoading(true);
            setError(null);

            const response = await api.get(`/api/v3/feemanagement/students/${studentNumber}/balance-summary`);
            setBalanceSummary(response.data);
        } catch (err) {
            setError('Failed to load student fee balance');
            console.error('Error loading student balance:', err);
        } finally {
            setLoading(false);
        }
    };

    const handlePaymentUpdate = async (balanceId, amount) => {
        try {
            setLoading(true);
            await api.put(`/api/v3/feemanagement/balances/${balanceId}/update-payment`, amount);
            await loadStudentBalance(); // Reload data
            setPaymentAmount('');
            setSelectedBalanceId(null);
            alert('Payment updated successfully!');
        } catch (err) {
            setError('Failed to update payment');
            console.error('Error updating payment:', err);
        } finally {
            setLoading(false);
        }
    };

    const getStatusColor = (status) => {
        switch (status) {
            case 'Paid':
                return 'text-green-600 bg-green-100';
            case 'Partial':
                return 'text-yellow-600 bg-yellow-100';
            case 'Overdue':
                return 'text-red-600 bg-red-100';
            default:
                return 'text-gray-600 bg-gray-100';
        }
    };

    if (loading) {
        return (
            <div className="flex justify-center items-center h-64">
                <Spinner />
            </div>
        );
    }

    if (!balanceSummary) {
        return (
            <Alert type="error" message="No balance information found for this student" />
        );
    }

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <h2 className="text-2xl font-bold text-gray-900">
                    Fee Balance - {balanceSummary.studentName}
                </h2>
                <div className="text-sm text-gray-500">
                    Student: {balanceSummary.studentNumber}
                </div>
            </div>

            {error && (
                <Alert type="error" message={error} />
            )}

            {/* Summary Cards */}
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                <Card>
                    <div className="text-center">
                        <h3 className="text-lg font-semibold text-red-600">
                            ${balanceSummary.totalOutstandingBalance.toLocaleString()}
                        </h3>
                        <p className="text-sm text-gray-600">Outstanding Balance</p>
                    </div>
                </Card>
                <Card>
                    <div className="text-center">
                        <h3 className="text-lg font-semibold text-green-600">
                            ${balanceSummary.totalPaid.toLocaleString()}
                        </h3>
                        <p className="text-sm text-gray-600">Total Paid</p>
                    </div>
                </Card>
                <Card>
                    <div className="text-center">
                        <h3 className="text-lg font-semibold text-blue-600">
                            {balanceSummary.feeBalances.length}
                        </h3>
                        <p className="text-sm text-gray-600">Fee Items</p>
                    </div>
                </Card>
                <Card>
                    <div className="text-center">
                        <h3 className="text-lg font-semibold text-purple-600">
                            {balanceSummary.additionalFees.length}
                        </h3>
                        <p className="text-sm text-gray-600">Additional Fees</p>
                    </div>
                </Card>
            </div>

            {/* Fee Balances */}
            <Card>
                <h3 className="text-xl font-semibold mb-4">Fee Structure Balances</h3>
                {balanceSummary.feeBalances.length === 0 ? (
                    <p className="text-gray-500">No fee balances found</p>
                ) : (
                    <div className="space-y-4">
                        {balanceSummary.feeBalances.map((balance) => (
                            <div key={balance.id} className="border border-gray-200 rounded-lg p-4">
                                <div className="flex justify-between items-start mb-3">
                                    <div>
                                        <h4 className="font-medium text-gray-900">{balance.feeCategoryName}</h4>
                                        <p className="text-sm text-gray-600">
                                            Due: {new Date(balance.dueDate).toLocaleDateString()}
                                        </p>
                                    </div>
                                    <div className="text-right">
                                        <span className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(balance.status)}`}>
                                            {balance.status}
                                        </span>
                                    </div>
                                </div>
                                
                                <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-3">
                                    <div>
                                        <p className="text-sm text-gray-600">Total Amount</p>
                                        <p className="font-semibold">${balance.totalAmount.toLocaleString()}</p>
                                    </div>
                                    <div>
                                        <p className="text-sm text-gray-600">Amount Paid</p>
                                        <p className="font-semibold text-green-600">${balance.amountPaid.toLocaleString()}</p>
                                    </div>
                                    <div>
                                        <p className="text-sm text-gray-600">Outstanding</p>
                                        <p className="font-semibold text-red-600">${balance.outstandingBalance.toLocaleString()}</p>
                                    </div>
                                </div>

                                {balance.outstandingBalance > 0 && user?.role === 'Admin' && (
                                    <div className="flex items-center space-x-2">
                                        <input
                                            type="number"
                                            placeholder="Payment amount"
                                            value={selectedBalanceId === balance.id ? paymentAmount : ''}
                                            onChange={(e) => {
                                                setSelectedBalanceId(balance.id);
                                                setPaymentAmount(e.target.value);
                                            }}
                                            className="flex-1 px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                                        />
                                        <Button
                                            onClick={() => handlePaymentUpdate(balance.id, parseFloat(paymentAmount))}
                                            disabled={!paymentAmount || parseFloat(paymentAmount) <= 0}
                                            size="sm"
                                        >
                                            Update Payment
                                        </Button>
                                    </div>
                                )}
                            </div>
                        ))}
                    </div>
                )}
            </Card>

            {/* Additional Fees */}
            {balanceSummary.additionalFees.length > 0 && (
                <Card>
                    <h3 className="text-xl font-semibold mb-4">Additional Fees</h3>
                    <div className="space-y-4">
                        {balanceSummary.additionalFees.map((fee) => (
                            <div key={fee.id} className="border border-gray-200 rounded-lg p-4">
                                <div className="flex justify-between items-start">
                                    <div>
                                        <h4 className="font-medium text-gray-900">{fee.additionalFeeName}</h4>
                                        <p className="text-sm text-gray-600">
                                            Due: {new Date(fee.dueDate).toLocaleDateString()}
                                        </p>
                                        <p className="text-sm text-gray-500">
                                            Assigned: {new Date(fee.assignedAt).toLocaleDateString()}
                                        </p>
                                    </div>
                                    <div className="text-right">
                                        <p className="font-semibold text-blue-600">${fee.amount.toLocaleString()}</p>
                                        <span className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(fee.status)}`}>
                                            {fee.status}
                                        </span>
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>
                </Card>
            )}

            {/* Next Payment Due */}
            <Card>
                <h3 className="text-xl font-semibold mb-4">Payment Schedule</h3>
                <div className="text-center">
                    <p className="text-sm text-gray-600">Next Payment Due</p>
                    <p className="text-2xl font-bold text-red-600">
                        {new Date(balanceSummary.nextPaymentDue).toLocaleDateString()}
                    </p>
                    <p className="text-sm text-gray-500 mt-2">
                        Outstanding Balance: ${balanceSummary.totalOutstandingBalance.toLocaleString()}
                    </p>
                </div>
            </Card>
        </div>
    );
};

export default StudentFeeBalance; 