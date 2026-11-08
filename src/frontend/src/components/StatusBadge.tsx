import React from 'react';

interface StatusBadgeProps {
  status: 'active' | 'paused' | 'error' | 'not_found' | 'SUCCESS' | 'FAILED' | 'SKIPPED' | string;
  label?: string;
  size?: 'sm' | 'md';
}

export const StatusBadge: React.FC<StatusBadgeProps> = ({ status, label, size = 'sm' }) => {
  const norm = status.toLowerCase();

  let bgClass = 'bg-[#272C36] text-[#A0A8B5] border-[#2B313E]';
  let dotClass = 'bg-[#A0A8B5]';
  let displayLabel = label || status;

  if (norm === 'active' || norm === 'success') {
    bgClass = 'bg-[rgba(53,183,122,0.12)] text-[#35B77A] border-[rgba(53,183,122,0.3)]';
    dotClass = 'bg-[#35B77A] shadow-[0_0_8px_#35B77A]';
    displayLabel = label || (norm === 'active' ? 'Active' : 'Success');
  } else if (norm === 'paused' || norm === 'skipped') {
    bgClass = 'bg-[rgba(233,174,84,0.12)] text-[#E9AE54] border-[rgba(233,174,84,0.3)]';
    dotClass = 'bg-[#E9AE54]';
    displayLabel = label || (norm === 'paused' ? 'Paused' : 'Skipped');
  } else if (norm === 'error' || norm === 'failed' || norm === 'not_found') {
    bgClass = 'bg-[rgba(227,111,117,0.12)] text-[#E36F75] border-[rgba(227,111,117,0.3)]';
    dotClass = 'bg-[#E36F75] shadow-[0_0_8px_#E36F75]';
    displayLabel = label || (norm === 'not_found' ? 'Missing' : norm === 'error' ? 'Error' : 'Failed');
  }

  const sizeClasses = size === 'sm' ? 'px-2 py-0.5 text-xs' : 'px-3 py-1 text-sm';

  return (
    <span className={`inline-flex items-center gap-1.5 rounded-full border font-medium ${sizeClasses} ${bgClass}`}>
      <span className={`w-1.5 h-1.5 rounded-full ${dotClass}`} />
      <span>{displayLabel}</span>
    </span>
  );
};

