import React, { useState, useEffect } from 'react';
import Card from '../common/Card';
import Button from '../common/Button';
import Alert from '../common/ErrorMessage';
import Spinner from '../common/LoadingSpinner';
import { useAuth } from '../../hooks/useAuth';
import api from '../../services/api';

const FeeManagementDashboard = () => {
    const { user } = useAuth();
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [stats, setStats] = useState({
        totalFeeStructures: 0,
        totalFeeCategories: 0,
        totalAdditionalFees: 0,
        studentsWithOutstandingFees: 0,
        totalOutstandingAmount: 0
    });
    const [recentFeeStructures, setRecentFeeStructures] = useState([]);
    const [recentAdditionalFees, setRecentAdditionalFees] = useState([]);

    useEffect(() => {
        loadDashboardData();
    }, []);

    const loadDashboardData = async () => {
        try {
            setLoading(true);
            setError(null);

            // Load recent fee structures
            const structuresResponse = await api.get('/api/v3/feemanagement/structures');
            const structures = structuresResponse.data.slice(0, 5); // Get latest 5

            // Load recent additional fees
            const additionalFeesResponse = await api.get('/api/v3/feemanagement/additional-fees');
            const additionalFees = additionalFeesResponse.data.slice(0, 5); // Get latest 5

            // Load fee categories for stats
            const categoriesResponse = await api.get('/api/v3/feemanagement/categories');
            const categories = categoriesResponse.data;

            setStats({
                totalFeeStructures: structures.length,
                totalFeeCategories: categories.length,
                totalAdditionalFees: additionalFees.length,
                studentsWithOutstandingFees: 0, // Will be updated when we add balance endpoints
                totalOutstandingAmount: 0 // Will be updated when we add balance endpoints
            });

            setRecentFeeStructures(structures);
            setRecentAdditionalFees(additionalFees);
        } catch (err) {
            setError('Failed to load fee management dashboard data');
            console.error('Error loading dashboard data:', err);
        } finally {
            setLoading(false);
        }
    };

    const handleSeedData = async () => {
        try {
            setLoading(true);
            await api.post('/api/v3/feemanagement/seed-data');
            await loadDashboardData(); // Reload data after seeding
            alert('Fee management data seeded successfully!');
        } catch (err) {
            setError('Failed to seed fee management data');
            console.error('Error seeding data:', err);
        } finally {
            setLoading(false);
        }
    };

    const handleApplyAdditionalFees = async () => {
        try {
            setLoading(true);
            await api.post('/api/v3/feemanagement/apply-additional-fees');
            await loadDashboardData(); // Reload data after applying
            alert('Additional fees applied successfully!');
        } catch (err) {
            setError('Failed to apply additional fees');
            console.error('Error applying additional fees:', err);
        } finally {
            setLoading(false);
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
            <div className="flex justify-between items-center">
                <h1 className="text-3xl font-bold text-gray-900">Fee Management Dashboard</h1>
                <div className="space-x-2">
                    <Button onClick={handleSeedData} variant="secondary">
                        Seed Data
                    </Button>
                    <Button onClick={handleApplyAdditionalFees} variant="secondary">
                        Apply Additional Fees
                    </Button>
                </div>
            </div>

            {error && (
                <Alert type="error" message={error} />
            )}

            {/* Statistics Cards */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4">
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
                <Card>
                    <div className="text-center">
                        <h3 className="text-lg font-semibold text-gray-900">{stats.totalAdditionalFees}</h3>
                        <p className="text-sm text-gray-600">Additional Fees</p>
                    </div>
                </Card>
                <Card>
                    <div className="text-center">
                        <h3 className="text-lg font-semibold text-red-600">{stats.studentsWithOutstandingFees}</h3>
                        <p className="text-sm text-gray-600">Outstanding Balances</p>
                    </div>
                </Card>
                <Card>
                    <div className="text-center">
                        <h3 className="text-lg font-semibold text-red-600">
                            ${stats.totalOutstandingAmount.toLocaleString()}
                        </h3>
                        <p className="text-sm text-gray-600">Total Outstanding</p>
                    </div>
                </Card>
            </div>

            {/* Recent Fee Structures */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
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

                {/* Recent Additional Fees */}
                <Card>
                    <h2 className="text-xl font-semibold mb-4">Recent Additional Fees</h2>
                    {recentAdditionalFees.length === 0 ? (
                        <p className="text-gray-500">No additional fees found</p>
                    ) : (
                        <div className="space-y-3">
                            {recentAdditionalFees.map((fee) => (
                                <div key={fee.id} className="border-b border-gray-200 pb-3 last:border-b-0">
                                    <div className="flex justify-between items-start">
                                        <div>
                                            <h4 className="font-medium text-gray-900">{fee.name}</h4>
                                            <p className="text-sm text-gray-600">{fee.description}</p>
                                            <p className="text-xs text-gray-500">
                                                {fee.applicableTo} • {fee.frequency}
                                            </p>
                                        </div>
                                        <div className="text-right">
                                            <p className="font-semibold text-blue-600">
                                                ${fee.amount.toLocaleString()}
                                            </p>
                                            <p className="text-xs text-gray-500">
                                                {fee.isActive ? 'Active' : 'Inactive'}
                                            </p>
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </Card>
            </div>

            {/* Quick Actions */}
            <Card>
                <h2 className="text-xl font-semibold mb-4">Quick Actions</h2>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                    <Button variant="outline" className="h-16">
                        <div className="text-center">
                            <div className="font-medium">Create Fee Structure</div>
                            <div className="text-sm text-gray-500">Set up new fee structure</div>
                        </div>
                    </Button>
                    <Button variant="outline" className="h-16">
                        <div className="text-center">
                            <div className="font-medium">Add Additional Fee</div>
                            <div className="text-sm text-gray-500">Create one-time fees</div>
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
        </div>
    );
};

export default FeeManagementDashboard; 