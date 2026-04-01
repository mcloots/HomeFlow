export interface HouseholdMemberListItem {
  memberId: string;
  displayName: string;
  email: string;
  role: string;
}

export interface GetHouseholdMembersResponse {
  householdId: string;
  members: HouseholdMemberListItem[];
}
