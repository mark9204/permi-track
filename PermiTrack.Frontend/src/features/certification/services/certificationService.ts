export interface CertificationCampaign {
  id: number;
  name: string;
  description: string;
  startDate: string;
  dueDate: string;
  status: 'Active' | 'Completed' | 'Expired';
  progress: number; // 0-100
  totalItems: number;
  reviewedItems: number;
}

export interface CertificationItem {
  id: number;
  userId: number;
  userName: string;
  roleName: string;
  department: string;
  lastLogin: string;
  status: 'Pending' | 'Kept' | 'Revoked';
  riskLevel: 'Low' | 'Medium' | 'High';
}

// --- DEMO STATE ---
const mockCampaigns: CertificationCampaign[] = [
  {
    id: 1,
    name: 'Q4 2025 IT Security Review',
    description: 'Quarterly review of all IT department access rights.',
    startDate: '2025-12-01',
    dueDate: '2025-12-15',
    status: 'Active',
    progress: 35,
    totalItems: 20,
    reviewedItems: 7
  },
  {
    id: 2,
    name: 'Annual HR Access Audit',
    description: 'Yearly certification of HR system permissions.',
    startDate: '2025-11-15',
    dueDate: '2025-12-31',
    status: 'Active',
    progress: 10,
    totalItems: 15,
    reviewedItems: 1
  },
  {
    id: 3,
    name: 'Q3 2025 General Review',
    description: 'Past quarter review.',
    startDate: '2025-09-01',
    dueDate: '2025-09-15',
    status: 'Completed',
    progress: 100,
    totalItems: 45,
    reviewedItems: 45
  }
];

const generateMockItems = (count: number): CertificationItem[] => {
  const users = ['John Doe', 'Alice Smith', 'Bob Jones', 'Sarah Connor', 'Mike Ross'];
  const roles = ['IT Admin', 'HR Manager', 'Finance Viewer', 'System Operator', 'Data Analyst'];
  const depts = ['IT', 'HR', 'Finance', 'Operations'];
  
  return Array.from({ length: count }).map((_, i) => ({
    id: i + 1,
    userId: 100 + i,
    userName: users[i % users.length],
    roleName: roles[i % roles.length],
    department: depts[i % depts.length],
    lastLogin: new Date(Date.now() - Math.floor(Math.random() * 1000000000)).toISOString(),
    status: i < 3 ? 'Kept' : 'Pending', // Pre-fill a few
    riskLevel: i % 5 === 0 ? 'High' : i % 3 === 0 ? 'Medium' : 'Low'
  }));
};

let mockItems = generateMockItems(20);

export const certificationService = {
  async getCampaigns(): Promise<CertificationCampaign[]> {
    await new Promise(resolve => setTimeout(resolve, 500));
    return [...mockCampaigns];
  },

  async getCampaignItems(campaignId: number): Promise<CertificationItem[]> {
    await new Promise(resolve => setTimeout(resolve, 600));
    console.log(`Fetching items for campaign ${campaignId}`);
    // In a real app, we'd filter by campaignId. For demo, just return the mock list.
    return [...mockItems];
  },

  async reviewItem(itemId: number, decision: 'Kept' | 'Revoked'): Promise<void> {
    await new Promise(resolve => setTimeout(resolve, 300));
    const item = mockItems.find(i => i.id === itemId);
    if (item) {
      item.status = decision;
      
      // Update campaign progress simulation
      const campaign = mockCampaigns[0]; // Just update the first one for demo
      campaign.reviewedItems++;
      campaign.progress = Math.round((campaign.reviewedItems / campaign.totalItems) * 100);
    }
  }
};
