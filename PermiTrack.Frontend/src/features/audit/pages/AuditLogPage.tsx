import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Table, Card, Tag, Typography, Button, Modal, Descriptions, DatePicker, Space, Alert, Row, Col, Statistic, Select } from 'antd';
import { EyeOutlined, SafetyCertificateOutlined, WarningOutlined, GlobalOutlined } from '@ant-design/icons';
import auditLogService from '../services/auditService';
import type { AuditLog } from '../services/auditService';
import type { PaginatedResponse } from '../../../types/api.types';

const { Text } = Typography;
const { RangePicker } = DatePicker;

const formatDateShort = (iso?: string) => {
  if (!iso) return '';
  try {
    const d = new Date(iso);
    return d.toLocaleString();
  } catch {
    return iso;
  }
};

const AuditLogPage: React.FC = () => {
  const [page, setPage] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(10);
  const [selectedLog, setSelectedLog] = useState<AuditLog | null>(null);
  const [dateRange, setDateRange] = useState<any>(null);
  const [riskFilter, setRiskFilter] = useState<string | undefined>(undefined);

  const dateFrom = dateRange?.[0]?.toISOString?.() ?? undefined;
  const dateTo = dateRange?.[1]?.toISOString?.() ?? undefined;

  const { data, isLoading, isError, error } = useQuery<PaginatedResponse<AuditLog>, Error>({
    queryKey: ['audit-logs', page, pageSize, dateFrom, dateTo, riskFilter],
    queryFn: () => auditLogService.getLogs({ page, pageSize, dateFrom, dateTo, riskLevel: riskFilter })
  });

  const tableData = Array.isArray(data?.data) ? data.data : [];
  const totalCount = data?.pagination?.totalCount ?? 0;

  // Calculate some quick stats for the header
  const highRiskCount = tableData.filter(l => l.riskScore === 'High' || l.riskScore === 'Critical').length;
  const uniqueLocations = new Set(tableData.map(l => l.location)).size;

  const onTableChange = (pagination: any) => {
    setPage(pagination.current);
    setPageSize(pagination.pageSize);
  };

  const columns = [
    {
      title: 'Time',
      dataIndex: 'createdAt',
      key: 'createdAt',
      render: (val: string) => <Text style={{ fontSize: '12px' }}>{formatDateShort(val)}</Text>,
      width: 160,
    },
    {
      title: 'Risk',
      dataIndex: 'riskScore',
      key: 'riskScore',
      render: (val: string) => {
        let color = 'green';
        if (val === 'Medium') color = 'orange';
        if (val === 'High') color = 'volcano';
        if (val === 'Critical') color = 'red';
        return <Tag color={color}>{val?.toUpperCase()}</Tag>;
      },
      width: 100,
    },
    {
      title: 'User',
      dataIndex: 'userName',
      key: 'userName',
      render: (val: string) => <Text strong>{val ?? 'System'}</Text>,
      width: 140,
    },
    {
      title: 'Action',
      dataIndex: 'action',
      key: 'action',
      render: (val: string) => <Tag>{val}</Tag>,
      width: 140,
    },
    {
      title: 'Location',
      dataIndex: 'location',
      key: 'location',
      render: (val: string) => (
        <Space size={4}>
          <GlobalOutlined />
          <Text>{val}</Text>
        </Space>
      ),
      width: 150,
    },
    {
      title: 'Behavior',
      dataIndex: 'behaviorTags',
      key: 'behaviorTags',
      render: (tags: string[]) => (
        <>
          {tags?.map(tag => (
            <Tag key={tag} color={tag === 'Normal' ? 'default' : 'warning'} style={{ fontSize: '10px' }}>
              {tag}
            </Tag>
          ))}
        </>
      ),
    },
    {
      title: 'Action',
      key: 'actions',
      render: (_: any, record: AuditLog) => (
        <Button size="small" icon={<EyeOutlined />} onClick={() => setSelectedLog(record)}>
          Details
        </Button>
      ),
      width: 100,
    },
  ];

  return (
    <div>
      <Row gutter={16} style={{ marginBottom: 24 }}>
        <Col span={8}>
          <Card>
            <Statistic 
              title="Security Events (24h)" 
              value={totalCount} 
              prefix={<SafetyCertificateOutlined />} 
            />
          </Card>
        </Col>
        <Col span={8}>
          <Card>
            <Statistic 
              title="High Risk Detected" 
              value={highRiskCount} 
              valueStyle={{ color: '#cf1322' }}
              prefix={<WarningOutlined />} 
            />
          </Card>
        </Col>
        <Col span={8}>
          <Card>
            <Statistic 
              title="Active Locations" 
              value={uniqueLocations} 
              prefix={<GlobalOutlined />} 
            />
          </Card>
        </Col>
      </Row>

      <Card 
        title="Audit Logs & Behavior Monitoring" 
        extra={
          <Space>
            <Select
              placeholder="Filter by Risk"
              style={{ width: 150 }}
              allowClear
              onChange={(val) => setRiskFilter(val)}
              options={[
                { label: 'Low Risk', value: 'Low' },
                { label: 'Medium Risk', value: 'Medium' },
                { label: 'High Risk', value: 'High' },
                { label: 'Critical Risk', value: 'Critical' },
              ]}
            />
            <RangePicker onChange={(dates) => setDateRange(dates)} />
          </Space>
        }
      >
        {isError && (
          <Alert
            message="Error"
            description={(error as any)?.message}
            type="error"
            showIcon
            style={{ marginBottom: 16 }}
          />
        )}

        <Table
          dataSource={tableData}
          columns={columns}
          rowKey="id"
          loading={isLoading}
          pagination={{
            current: page,
            pageSize: pageSize,
            total: totalCount,
            showSizeChanger: true
          }}
          onChange={onTableChange}
          size="middle"
        />
      </Card>

      <Modal
        title="Log Details"
        open={!!selectedLog}
        onCancel={() => setSelectedLog(null)}
        footer={[
          <Button key="close" onClick={() => setSelectedLog(null)}>
            Close
          </Button>
        ]}
        width={700}
      >
        {selectedLog && (
          <Descriptions bordered column={1} size="small">
            <Descriptions.Item label="Log ID">{selectedLog.id}</Descriptions.Item>
            <Descriptions.Item label="Timestamp">{formatDateShort(selectedLog.createdAt)}</Descriptions.Item>
            <Descriptions.Item label="Risk Score">
              <Tag color={selectedLog.riskScore === 'Critical' ? 'red' : selectedLog.riskScore === 'High' ? 'volcano' : 'green'}>
                {selectedLog.riskScore}
              </Tag>
            </Descriptions.Item>
            <Descriptions.Item label="User">{selectedLog.userName} (ID: {selectedLog.userId})</Descriptions.Item>
            <Descriptions.Item label="Action">{selectedLog.action}</Descriptions.Item>
            <Descriptions.Item label="Resource">{selectedLog.resourceType} (ID: {selectedLog.resourceId})</Descriptions.Item>
            <Descriptions.Item label="IP Address">{selectedLog.ipAddress}</Descriptions.Item>
            <Descriptions.Item label="Location">{selectedLog.location}</Descriptions.Item>
            <Descriptions.Item label="Device">{selectedLog.device}</Descriptions.Item>
            <Descriptions.Item label="Behavior Tags">
              {selectedLog.behaviorTags?.join(', ')}
            </Descriptions.Item>
            <Descriptions.Item label="Changes">
              <pre style={{ fontSize: '11px', maxHeight: '100px', overflow: 'auto' }}>
                {JSON.stringify({ old: selectedLog.oldValues, new: selectedLog.newValues }, null, 2)}
              </pre>
            </Descriptions.Item>
          </Descriptions>
        )}
      </Modal>
    </div>
  );
};

export default AuditLogPage;
