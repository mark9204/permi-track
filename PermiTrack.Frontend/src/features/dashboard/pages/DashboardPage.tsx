import React from 'react';
import { useQuery } from '@tanstack/react-query';
import { Card, Statistic, Row, Col, List, Spin, Alert, Typography } from 'antd';
import { FileOutlined, ClockCircleOutlined, CheckCircleOutlined, ExclamationCircleOutlined } from '@ant-design/icons';
import dashboardService from '../services/dashboardService';
import type { DashboardStats } from '../services/dashboardService';
import { useAuthStore } from '../../../stores/authStore';

const { Title, Text } = Typography;

const DashboardPage: React.FC = () => {
  const { user } = useAuthStore();

  const { data, isLoading, isError, error } = useQuery<DashboardStats, Error>({ queryKey: ['dashboard-stats'], queryFn: dashboardService.getStats });

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

  const stats: DashboardStats =
    data ?? {
      total: 0,
      pending: 0,
      approved: 0,
      rejected: 0,
      cancelled: 0,
      averageProcessingTimeHours: 0,
      topRequestedRoles: [],
    };

  return (
    <div>
      <Row style={{ marginBottom: 16 }}>
        <Col span={24}>
          <Title level={3}>Welcome, {user?.firstName || user?.username || 'User'}!</Title>
          <Text type="secondary">Quick overview of access request activity</Text>
        </Col>
      </Row>

      <Row gutter={16} style={{ marginTop: 16 }}>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic title="Total Requests" value={stats.total} prefix={<FileOutlined />} />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Pending"
              value={stats.pending}
              valueStyle={{ color: '#fa8c16' }}
              prefix={<ExclamationCircleOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Approved"
              value={stats.approved}
              valueStyle={{ color: '#52c41a' }}
              prefix={<CheckCircleOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Rejected"
              value={stats.rejected}
              valueStyle={{ color: '#f5222d' }}
              prefix={<ExclamationCircleOutlined />}
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

        <Col xs={24} lg={8}>
          <Card>
            <Statistic
              title="Avg Processing Time"
              value={Number(stats.averageProcessingTimeHours.toFixed(2))}
              suffix="hrs"
              prefix={<ClockCircleOutlined />}
            />
          </Card>
        </Col>
      </Row>
    </div>
  );
};

export default DashboardPage;
