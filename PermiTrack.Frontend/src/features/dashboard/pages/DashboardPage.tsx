import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { Card, Statistic, Row, Col, List, Spin, Alert, Typography, Tag, Space } from 'antd';
import { FileOutlined, CheckCircleOutlined, ExclamationCircleOutlined, CloseCircleOutlined } from '@ant-design/icons';
import dashboardService from '../services/dashboardService';
import type { DashboardStats } from '../services/dashboardService';
import { useAuthStore } from '../../../stores/authStore';

const { Title, Text } = Typography;

const DashboardPage: React.FC = () => {
  const { user } = useAuthStore();

  // Determine if user is admin
  const isAdmin = React.useMemo(() => {
    if (!user?.roles) return false;
    const roles = Array.isArray(user.roles) ? user.roles : [user.roles];
    return roles.some((r: any) => String(r).toLowerCase() === 'admin');
  }, [user]);

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
        message="Failed to load dashboard"
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
    viewMode: 'Personal'
  };

  return (
    <div>
      <Row style={{ marginBottom: 16 }}>
        <Col span={24}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <div>
              <Title level={3} style={{ marginBottom: 0 }}>Welcome, {user?.firstName || user?.username || 'User'}!</Title>
              <Text type="secondary">Quick overview of access request activity</Text>
            </div>
            <Tag color={stats.viewMode === 'Global' ? 'purple' : 'blue'} style={{ fontSize: '14px', padding: '4px 10px' }}>
              {stats.viewMode} View
            </Tag>
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
        <Col xs={24} lg={16}>
          <Card title="Top Requested Roles">
            <List
              dataSource={stats.topRequestedRoles}
              locale={{ emptyText: 'No roles requested yet' }}
              renderItem={(item: any) => (
                <List.Item>
                  <List.Item.Meta
                    title={item.roleName}
                    description={<Text type="secondary">Requested {item.count} times</Text>}
                  />
                </List.Item>
              )}
            />
          </Card>
        </Col>
      </Row>
    </div>
  );
};

export default DashboardPage;
