import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Card, Statistic, Row, Col, Spin, Alert, Typography, Tag, Space, Button, Table } from 'antd';
import { FileOutlined, CheckCircleOutlined, ExclamationCircleOutlined, CloseCircleOutlined, PlusOutlined } from '@ant-design/icons';
import { PieChart, Pie, Cell, Tooltip as RechartsTooltip, Legend, ResponsiveContainer, BarChart, Bar, XAxis, YAxis, CartesianGrid } from 'recharts';
import dashboardService from '../services/dashboardService';
import type { DashboardStats } from '../services/dashboardService';
import { useAuthStore } from '../../../stores/authStore';
import { useUserPermissions } from '../../../hooks/useUserPermissions';

const { Title, Text } = Typography;

const DashboardPage: React.FC = () => {
  const { user } = useAuthStore();
  const navigate = useNavigate();
  const { isAdmin } = useUserPermissions();

  const { data, isLoading, isError, error } = useQuery<DashboardStats, Error>({
    queryKey: ['dashboard-stats', isAdmin],
    queryFn: () => dashboardService.getStats(isAdmin)
  });

  if (isLoading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: 240 }}>
        <Spin size="large" />
      </div>
    );
  }

  if (isError) {
    return (
      <Alert
        title="Failed to load dashboard"
        description={(error as any)?.message || 'An error occurred while fetching statistics.'}
        type="error"
        showIcon
      />
    );
  }

  const stats: DashboardStats = data ?? {
    total: 0,
    pending: 0,
    approved: 0,
    rejected: 0,
    cancelled: 0,
    topRequestedRoles: [],
    latestRequests: [],
    viewMode: 'Personal',
    requestsByStatus: [],
    requestsOverTime: []
  };

  const columns = [
    {
      title: 'Role',
      dataIndex: 'requestedRoleName',
      key: 'role',
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => {
        let color = 'default';
        if (status === 'Pending') color = 'processing';
        if (status === 'Approved') color = 'success';
        if (status === 'Rejected') color = 'error';
        return <Tag color={color}>{status}</Tag>;
      }
    },
    {
      title: 'Date',
      dataIndex: 'requestedAt',
      key: 'date',
      render: (date: string) => new Date(date).toLocaleDateString(),
    },
  ];

  return (
    <div>
      <Row style={{ marginBottom: 16 }}>
        <Col span={24}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <div>
              <Title level={3} style={{ marginBottom: 0 }}>Welcome, {user?.firstName || user?.username || 'User'}!</Title>
              <Text type="secondary">Quick overview of access request activity</Text>
            </div>
            <Space>
              <Button 
                type="primary" 
                icon={<PlusOutlined />} 
                onClick={() => navigate('/my-access', { state: { openCreateModal: true } })}
              >
                Create Request
              </Button>
              <Tag color={stats.viewMode === 'Global' ? 'purple' : 'blue'} style={{ fontSize: '14px', padding: '4px 10px' }}>
                {stats.viewMode} View
              </Tag>
            </Space>
          </div>
        </Col>
      </Row>

      <Row gutter={[16, 16]} style={{ marginTop: 16 }}>
        <Col xs={24} sm={12} md={4}>
          <Card>
            <Statistic title="Total Requests" value={stats.total} prefix={<FileOutlined />} />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={5}>
          <Card>
            <Statistic
              title="Pending"
              value={stats.pending}
              valueStyle={{ color: '#fa8c16' }}
              prefix={<ExclamationCircleOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={5}>
          <Card>
            <Statistic
              title="Approved"
              value={stats.approved}
              valueStyle={{ color: '#52c41a' }}
              prefix={<CheckCircleOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={5}>
          <Card>
            <Statistic
              title="Rejected"
              value={stats.rejected}
              valueStyle={{ color: '#f5222d' }}
              prefix={<ExclamationCircleOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={5}>
          <Card>
            <Statistic
              title="Cancelled"
              value={stats.cancelled}
              valueStyle={{ color: '#8c8c8c' }}
              prefix={<CloseCircleOutlined />}
            />
          </Card>
        </Col>
      </Row>

      <Row gutter={16} style={{ marginTop: 24 }}>
        <Col xs={24} lg={12}>
          <Card title="Request Status Distribution">
            <div style={{ height: 300 }}>
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={stats.requestsByStatus}
                    cx="50%"
                    cy="50%"
                    innerRadius={60}
                    outerRadius={80}
                    paddingAngle={5}
                    dataKey="value"
                  >
                    {stats.requestsByStatus?.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={entry.color} />
                    ))}
                  </Pie>
                  <RechartsTooltip />
                  <Legend />
                </PieChart>
              </ResponsiveContainer>
            </div>
          </Card>
        </Col>
        <Col xs={24} lg={12}>
          <Card title="Activity Trend (Last 7 Days)">
            <div style={{ height: 300 }}>
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={stats.requestsOverTime}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="date" />
                  <YAxis allowDecimals={false} />
                  <RechartsTooltip />
                  <Bar dataKey="count" fill="#1890ff" name="Requests" />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </Card>
        </Col>
      </Row>

      <Row gutter={16} style={{ marginTop: 24 }}>
        <Col xs={24} lg={16}>
          <Card title="Latest Requests">
            <Table
              dataSource={stats.latestRequests}
              columns={columns}
              rowKey="id"
              pagination={false}
              locale={{ emptyText: 'No requests found' }}
            />
          </Card>
        </Col>
      </Row>
    </div>
  );
};

export default DashboardPage;
