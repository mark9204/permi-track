import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Table, Card, Tag, Button, Progress, Typography } from 'antd';
import { PlayCircleOutlined, CheckCircleOutlined } from '@ant-design/icons';
import { certificationService } from '../services/certificationService';
import type { CertificationCampaign } from '../services/certificationService';

const { Text } = Typography;

const CertificationListPage: React.FC = () => {
  const navigate = useNavigate();

  const { data: campaigns = [], isLoading } = useQuery({
    queryKey: ['certification-campaigns'],
    queryFn: certificationService.getCampaigns
  });

  const columns = [
    {
      title: 'Campaign Name',
      dataIndex: 'name',
      key: 'name',
      render: (text: string, record: CertificationCampaign) => (
        <div>
          <Text strong>{text}</Text>
          <br />
          <Text type="secondary" style={{ fontSize: '12px' }}>{record.description}</Text>
        </div>
      )
    },
    {
      title: 'Due Date',
      dataIndex: 'dueDate',
      key: 'dueDate',
      render: (val: string) => new Date(val).toLocaleDateString()
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => {
        let color = 'default';
        if (status === 'Active') color = 'processing';
        if (status === 'Completed') color = 'success';
        if (status === 'Expired') color = 'error';
        return <Tag color={color}>{status}</Tag>;
      }
    },
    {
      title: 'Progress',
      key: 'progress',
      width: 200,
      render: (_: any, record: CertificationCampaign) => (
        <div style={{ width: 180 }}>
          <Progress percent={record.progress} size="small" status={record.status === 'Completed' ? 'success' : 'active'} />
          <div style={{ fontSize: '11px', color: '#888' }}>
            {record.reviewedItems} / {record.totalItems} reviewed
          </div>
        </div>
      )
    },
    {
      title: 'Action',
      key: 'action',
      render: (_: any, record: CertificationCampaign) => (
        <Button 
          type={record.status === 'Active' ? 'primary' : 'default'}
          icon={record.status === 'Active' ? <PlayCircleOutlined /> : <CheckCircleOutlined />}
          onClick={() => navigate(`/certifications/${record.id}`)}
          disabled={record.status === 'Expired'}
        >
          {record.status === 'Active' ? 'Start Review' : 'View Details'}
        </Button>
      )
    }
  ];

  return (
    <Card title="Access Certification Campaigns">
      <Table
        dataSource={campaigns}
        columns={columns}
        rowKey="id"
        loading={isLoading}
        pagination={false}
      />
    </Card>
  );
};

export default CertificationListPage;
