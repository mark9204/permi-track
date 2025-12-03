import React from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Table, Card, Button, Tag, Space, Typography, message, Alert, Progress, Tooltip } from 'antd';
import { CheckOutlined, CloseOutlined, ArrowLeftOutlined } from '@ant-design/icons';
import { certificationService } from '../services/certificationService';
import type { CertificationItem } from '../services/certificationService';

const { Title, Text } = Typography;

const CertificationReviewPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const campaignId = Number(id);

  const { data: items = [], isLoading } = useQuery({
    queryKey: ['certification-items', campaignId],
    queryFn: () => certificationService.getCampaignItems(campaignId)
  });

  const reviewMutation = useMutation({
    mutationFn: ({ itemId, decision }: { itemId: number; decision: 'Kept' | 'Revoked' }) => 
      certificationService.reviewItem(itemId, decision),
    onSuccess: () => {
      message.success('Decision saved');
      queryClient.invalidateQueries({ queryKey: ['certification-items'] });
      queryClient.invalidateQueries({ queryKey: ['certification-campaigns'] });
    }
  });

  const pendingCount = items.filter(i => i.status === 'Pending').length;
  const totalCount = items.length;
  const progress = totalCount > 0 ? Math.round(((totalCount - pendingCount) / totalCount) * 100) : 0;

  const columns = [
    {
      title: 'User',
      dataIndex: 'userName',
      key: 'userName',
      render: (text: string, record: CertificationItem) => (
        <div>
          <Text strong>{text}</Text>
          <br />
          <Text type="secondary" style={{ fontSize: '12px' }}>{record.department}</Text>
        </div>
      )
    },
    {
      title: 'Role / Permission',
      dataIndex: 'roleName',
      key: 'roleName',
    },
    {
      title: 'Risk',
      dataIndex: 'riskLevel',
      key: 'riskLevel',
      render: (risk: string) => {
        let color = 'green';
        if (risk === 'Medium') color = 'orange';
        if (risk === 'High') color = 'red';
        return <Tag color={color}>{risk}</Tag>;
      }
    },
    {
      title: 'Last Login',
      dataIndex: 'lastLogin',
      key: 'lastLogin',
      render: (val: string) => new Date(val).toLocaleDateString(),
      responsive: ['md'] as any
    },
    {
      title: 'Decision',
      key: 'decision',
      render: (_: any, record: CertificationItem) => {
        if (record.status !== 'Pending') {
          return (
            <Tag icon={record.status === 'Kept' ? <CheckOutlined /> : <CloseOutlined />} color={record.status === 'Kept' ? 'success' : 'error'}>
              {record.status}
            </Tag>
          );
        }
        return <Tag color="default">Pending</Tag>;
      }
    },
    {
      title: 'Action',
      key: 'action',
      render: (_: any, record: CertificationItem) => (
        <Space>
          <Tooltip title="Keep Access">
            <Button 
              type="text" 
              shape="circle"
              icon={<CheckOutlined style={{ color: '#52c41a' }} />} 
              onClick={() => reviewMutation.mutate({ itemId: record.id, decision: 'Kept' })}
              disabled={record.status !== 'Pending'}
            />
          </Tooltip>
          <Tooltip title="Revoke Access">
            <Button 
              type="text" 
              shape="circle"
              icon={<CloseOutlined style={{ color: '#ff4d4f' }} />} 
              onClick={() => reviewMutation.mutate({ itemId: record.id, decision: 'Revoked' })}
              disabled={record.status !== 'Pending'}
            />
          </Tooltip>
        </Space>
      )
    }
  ];

  return (
    <div>
      <Button icon={<ArrowLeftOutlined />} onClick={() => navigate('/certifications')} style={{ marginBottom: 16 }}>
        Back to Campaigns
      </Button>

      <Card>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 24 }}>
          <div>
            <Title level={4} style={{ margin: 0 }}>Review Access Rights</Title>
            <Text type="secondary">Please review the access for the following users.</Text>
          </div>
          <div style={{ width: 200, textAlign: 'right' }}>
            <Text strong>{progress}% Complete</Text>
            <Progress percent={progress} showInfo={false} />
          </div>
        </div>

        {pendingCount === 0 && totalCount > 0 && (
          <Alert
            message="Review Completed"
            description="All items have been reviewed. You can now sign off on this campaign."
            type="success"
            showIcon
            style={{ marginBottom: 16 }}
            action={
              <Button type="primary" onClick={() => message.success('Campaign Signed Off!')}>
                Sign Off
              </Button>
            }
          />
        )}

        <Table
          dataSource={items}
          columns={columns}
          rowKey="id"
          loading={isLoading}
          pagination={{ pageSize: 10 }}
        />
      </Card>
    </div>
  );
};

export default CertificationReviewPage;
