export interface GmailConnection {
  gmailConnectionId: string;
  tenantId: string;
  householdId: string;
  googleEmail: string;
  status: string;
  connectedAtUtc: string;
}

export interface AppointmentSuggestion {
  sourceMessageId: string;
  sourceMessageDateUtc: string;
  sender: string;
  subject: string;
  suggestedTitle: string;
  suggestedStartsAtUtc: string | null;
  suggestedEndsAtUtc: string | null;
  suggestedLocation: string | null;
  suggestedDescription: string | null;
  confidence: number;
  reason: string;
}

export interface ScanGmailSuggestionsResponse {
  householdId: string;
  fromUtc: string;
  toUtc: string;
  suggestions: AppointmentSuggestion[];
}