import React, { useState, useEffect } from 'react';
import Card from '../components/common/Card';
import Button from '../components/common/Button';
import Alert from '../components/common/ErrorMessage';
import Spinner from '../components/common/LoadingSpinner';
import NotificationModal from '../components/common/NotificationModal';
import Modal from '../components/common/Modal';
import { useAuth } from '../hooks/useAuth';
import { feeManagementService } from '../services/feeManagementService';
import api from '../services/api';

const FeeManagementDashboard = () => {
    const { user } = useAuth();
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [notification, setNotification] = useState({
        isOpen: false,
        type: 'info',
        title: '',
        message: '',
        details: ''
    });
    const [stats, setStats] = useState({
        totalFeeStructures: 0,
        totalFeeCategories: 0
    });
    const [recentFeeStructures, setRecentFeeStructures] = useState([]);
    const [showAssignModal, setShowAssignModal] = useState(false);
    const [selectedFeeStructureId, setSelectedFeeStructureId] = useState('');
    const [allFeeStructures, setAllFeeStructures] = useState([]);
    const [assignLoading, setAssignLoading] = useState(false);

    useEffect(() => {
        loadDashboardData();
    }, []);

    // Fetch fee structures for modal
    useEffect(() => {
        const fetchFeeStructures = async () => {
            try {
                const structures = await api.get('/api/v3/feemanagement/structures/all');
                setAllFeeStructures(structures.data || []);
            } catch (err) {
                console.error('Error fetching fee structures:', err);
            }
        };
        if (showAssignModal) fetchFeeStructures();
    }, [showAssignModal]);

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

    const loadDashboardData = async () => {
        try {
            setLoading(true);
            setError(null);

            // Load recent fee structures
            const structuresResponse = await api.get('/api/v3/feemanagement/structures');
            const structures = structuresResponse.data.slice(0, 5); // Get latest 5

            // Load fee categories for stats
            const categoriesResponse = await api.get('/api/v3/feemanagement/categories');
            const categories = categoriesResponse.data;

            setStats({
                totalFeeStructures: structures.length,
                totalFeeCategories: categories.length
            });

            setRecentFeeStructures(structures);
        } catch (err) {
            setError('Failed to load fee management dashboard data');
            console.error('Error loading dashboard data:', err);
        } finally {
            setLoading(false);
        }
    };

    const handleApplyAdditionalFees = async () => {
        try {
            setLoading(true);
            const response = await feeManagementService.applyAdditionalFees();
            showNotification('success', 'Success', 'Additional fees applied successfully');
            loadDashboardData();
        } catch (error) {
            console.error('Error applying additional fees:', error);
            const errorMessage = error.response?.data?.message || error.message || 'Failed to apply additional fees';
            showNotification('error', 'Error', errorMessage);
        } finally {
            setLoading(false);
        }
    };

    const handleMigrateOldBalances = async () => {
        try {
            setLoading(true);
            const response = await feeManagementService.migrateOldBalances();
            showNotification('success', 'Migration Complete', 'Old student balances migrated successfully to new system');
            loadDashboardData();
        } catch (error) {
            console.error('Error migrating old balances:', error);
            const errorMessage = error.response?.data?.message || error.message || 'Failed to migrate old student balances';
            showNotification('error', 'Migration Failed', errorMessage);
        } finally {
            setLoading(false);
        }
    };

    const handleDebugBalances = async () => {
        try {
            setLoading(true);
            const response = await feeManagementService.debugBalanceTables();
            showNotification('success', 'Debug Complete', 'Balance table status checked');
            console.log('Balance table status:', response);
        } catch (error) {
            console.error('Error debugging balances:', error);
            const errorMessage = error.response?.data?.message || error.message || 'Failed to check balance table status';
            showNotification('error', 'Debug Failed', errorMessage);
        } finally {
            setLoading(false);
        }
    };

    const handleTestDatabase = async () => {
        try {
            setLoading(true);
            const response = await feeManagementService.testDatabase();
            showNotification('success', 'Database Test', 'Database connectivity successful');
            console.log('Database test response:', response);
        } catch (error) {
            console.error('Error testing database:', error);
            const errorMessage = error.response?.data?.message || error.message || 'Failed to connect to the database';
            showNotification('error', 'Database Test Failed', errorMessage);
        } finally {
            setLoading(false);
        }
    };

    const handleReconcileFeeBalances = async () => {
        try {
            setLoading(true);
            const response = await feeManagementService.reconcileFeeBalances();
            showNotification('success', 'Reconciliation Complete', response.message || 'Fee balances reconciled successfully');
        } catch (error) {
            const errorMessage = error.response?.data?.error || error.message || 'Failed to reconcile fee balances';
            showNotification('error', 'Reconciliation Failed', errorMessage);
        } finally {
            setLoading(false);
        }
    };

    const handleAssignFeeStructureToAll = async () => {
        try {
            setLoading(true);
            const response = await feeManagementService.assignFeeStructureToAllStudents();
            showNotification('success', 'Assignment Complete', response.message || 'Fee structure assigned to all students');
        } catch (error) {
            const errorMessage = error.response?.data?.error || error.message || 'Failed to assign fee structure';
            showNotification('error', 'Assignment Failed', errorMessage);
        } finally {
            setLoading(false);
        }
    };

    const handleOpenAssignModal = () => setShowAssignModal(true);
    const handleCloseAssignModal = () => {
        setShowAssignModal(false);
        setSelectedFeeStructureId('');
    };

    const handleAssignToAll = async () => {
        if (!selectedFeeStructureId) {
            showNotification('error', 'Selection Required', 'Please select a fee structure');
            return;
        }

        setAssignLoading(true);
        try {
            const response = await feeManagementService.assignFeeStructureToAll(parseInt(selectedFeeStructureId));
            showNotification('success', 'Assignment Complete', 
                `${response.message}. ${response.outstandingBalancesAdded > 0 ? 
                    `Added outstanding balances from ${response.outstandingBalancesAdded} students (${response.totalOutstandingAmount.toFixed(2)} total).` : ''}`);
            setShowAssignModal(false);
            setSelectedFeeStructureId('');
        } catch (error) {
            const errorMessage = error.response?.data?.error || error.message || 'Failed to assign fee structure';
            showNotification('error', 'Assignment Failed', errorMessage);
        } finally {
            setAssignLoading(false);
        }
    };


    if (loading) {
        return (
            <div className="flex justify-center items-center h-64">
                <Spinner />
            </div>
        );
    }

    return (
        <div className="space-y-6">
            {/* Header with Actions */}
            <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 mb-8">
              <div>
                <h1 className="text-3xl font-bold text-gray-900 mb-2">Fee Management Dashboard</h1>
                <p className="text-gray-600">Manage fee structures, categories, and student balances</p>
              </div>
              <div className="flex flex-wrap gap-3">
                <Button
                  variant="primary"
                  onClick={handleApplyAdditionalFees}
                  disabled={loading}
                  className="flex items-center gap-2"
                >
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                  </svg>
                  Apply Additional Fees
                </Button>
                <Button
                  variant="secondary"
                  onClick={handleMigrateOldBalances}
                  disabled={loading}
                  className="flex items-center gap-2"
                >
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                  </svg>
                  Migrate Old Balances
                </Button>
                <Button
                  variant="outline"
                  onClick={handleDebugBalances}
                  disabled={loading}
                  className="flex items-center gap-2"
                >
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                  </svg>
                  Debug Balances
                </Button>
                <Button
                  variant="outline"
                  onClick={handleTestDatabase}
                  disabled={loading}
                  className="flex items-center gap-2"
                >
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                  Test DB
                </Button>
              </div>
            </div>

            {error && (
                <Alert type="error" message={error} />
            )}

            {/* Statistics Cards */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <Card>
                    <div className="text-center">
                        <h3 className="text-lg font-semibold text-gray-900">{stats.totalFeeStructures}</h3>
                        <p className="text-sm text-gray-600">Fee Structures</p>
                    </div>
                </Card>
                <Card>
                    <div className="text-center">
                        <h3 className="text-lg font-semibold text-gray-900">{stats.totalFeeCategories}</h3>
                        <p className="text-sm text-gray-600">Fee Categories</p>
                    </div>
                </Card>
            </div>

            {/* Recent Fee Structures */}
            <Card>
                <h2 className="text-xl font-semibold mb-4">Recent Fee Structures</h2>
                {recentFeeStructures.length === 0 ? (
                    <p className="text-gray-500">No fee structures found</p>
                ) : (
                    <div className="space-y-3">
                        {recentFeeStructures.map((structure) => (
                            <div key={structure.id} className="border-b border-gray-200 pb-3 last:border-b-0">
                                <div className="flex justify-between items-start">
                                    <div>
                                        <h4 className="font-medium text-gray-900">{structure.name}</h4>
                                        <p className="text-sm text-gray-600">
                                            {structure.academicYear} - {structure.semester}
                                        </p>
                                        <p className="text-sm text-gray-500">{structure.description}</p>
                                    </div>
                                    <div className="text-right">
                                        <p className="font-semibold text-green-600">
                                            ${structure.totalAmount.toLocaleString()}
                                        </p>
                                        <p className="text-xs text-gray-500">
                                            {structure.feeStructureItems.length} items
                                        </p>
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </Card>

            {/* Quick Actions */}
            <Card>
                <h2 className="text-xl font-semibold mb-4">Quick Actions</h2>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <Button 
                        variant="outline" 
                        className="h-16"
                        onClick={() => window.location.href = '/admin/fee-structures'}
                    >
                        <div className="text-center">
                            <div className="font-medium">Create Fee Structure</div>
                            <div className="text-sm text-gray-500">Set up new fee structure</div>
                        </div>
                    </Button>
                    {/* TEMPORARY: Always show Reconcile Button for testing */}
                    <Button
                        variant="secondary"
                        className="h-16"
                        onClick={handleReconcileFeeBalances}
                        disabled={loading}
                    >
                        <div className="text-center">
                            <div className="font-medium">Reconcile Fee Balances</div>
                            <div className="text-sm text-gray-500">Sync balances with payments</div>
                        </div>
                    </Button>
                    {/* TEMPORARY: Always show Assign Fee Structure Button for admin */}
                    <Button
                        variant="secondary"
                        className="h-16"
                        onClick={handleAssignFeeStructureToAll}
                        disabled={loading}
                    >
                        <div className="text-center">
                            <div className="font-medium">Assign Summer 2025 Fee Structure</div>
                            <div className="text-sm text-gray-500">Bulk assign to all students</div>
                        </div>
                    </Button>
                    <Button
                        variant="secondary"
                        className="h-16"
                        onClick={handleOpenAssignModal}
                        disabled={loading}
                    >
                        <div className="text-center">
                            <div className="font-medium">Assign Fee Structure to All Students</div>
                            <div className="text-sm text-gray-500">Select fee structure and assign to all students</div>
                        </div>
                    </Button>
                    <Button variant="outline" className="h-16">
                        <div className="text-center">
                            <div className="font-medium">View Reports</div>
                            <div className="text-sm text-gray-500">Financial reports</div>
                        </div>
                    </Button>
                </div>
            </Card>
            <NotificationModal
                isOpen={notification.isOpen}
                onClose={closeNotification}
                type={notification.type}
                title={notification.title}
                message={notification.message}
                details={notification.details}
            />
            {/* Simple Assignment Modal */}
            {showAssignModal && (
                <Modal isOpen={showAssignModal} onClose={handleCloseAssignModal} title="Assign Fee Structure to All Students">
                    <div className="space-y-4">
                        <div className="bg-blue-50 p-4 rounded-lg">
                            <h4 className="font-medium text-blue-900 mb-2">Important Note:</h4>
                            <p className="text-blue-800 text-sm">
                                This will assign the selected fee structure to ALL students who don't already have it. 
                                Any outstanding balances from previous semesters will be added to the new fee structure.
                            </p>
                        </div>
                        
                        <div>
                            <label className="block text-sm font-medium mb-1">Select Fee Structure</label>
                            <select 
                                className="w-full border rounded p-2" 
                                value={selectedFeeStructureId} 
                                onChange={e => setSelectedFeeStructureId(e.target.value)}
                            >
                                <option value="">Choose a fee structure...</option>
                                {allFeeStructures.map(fs => (
                                    <option key={fs.id} value={fs.id}>
                                        {fs.name} ({fs.academicYear} {fs.semester})
                                    </option>
                                ))}
                            </select>
                        </div>

                        <div className="flex gap-4 mt-6">
                            <Button 
                                variant="primary" 
                                onClick={handleAssignToAll} 
                                disabled={assignLoading || !selectedFeeStructureId}
                                className="flex-1"
                            >
                                {assignLoading ? 'Assigning...' : 'Assign to All Students'}
                            </Button>
                            <Button variant="outline" onClick={handleCloseAssignModal} className="flex-1">
                                Cancel
                            </Button>
                        </div>
                    </div>
                </Modal>
            )}
        </div>
    );
};

export default FeeManagementDashboard; 