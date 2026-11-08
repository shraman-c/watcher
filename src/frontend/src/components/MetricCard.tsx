import React from 'react';
import { LucideIcon } from 'lucide-react';

interface MetricCardProps {
  title: string;
  value: string | number;
  subtitle?: string;
  icon: LucideIcon;
  iconColor?: string;
  iconBg?: string;
  badge?: { text: string; positive?: boolean };
}

export const MetricCard: React.FC<MetricCardProps> = ({
  title,
  value,
  subtitle,
  icon: Icon,
  iconColor = '#6C9BFF',
  iconBg = 'rgba(108, 155, 255, 0.12)',
  badge,
}) => {
  return (
    <div className="bg-[#20242D] border border-[#2B313E] rounded-xl p-4.5 hover:border-[#6C9BFF]/40 transition-all duration-200 shadow-subtle relative overflow-hidden group">
      <div className="flex items-center justify-between mb-3">
        <span className="text-xs font-semibold tracking-wide text-[#A0A8B5] uppercase">{title}</span>
        <div
          className="w-9 h-9 rounded-lg flex items-center justify-center transition-transform group-hover:scale-110"
          style={{ backgroundColor: iconBg, color: iconColor }}
        >
          <Icon size={18} />
        </div>
      </div>

      <div className="flex items-baseline gap-2">
        <span className="text-2xl font-bold text-[#F3F5F8] tracking-tight">{value}</span>
        {badge && (
          <span
            className={`text-xs px-1.5 py-0.5 rounded font-medium ${
              badge.positive
                ? 'bg-[#35B77A]/15 text-[#35B77A]'
                : 'bg-[#E36F75]/15 text-[#E36F75]'
            }`}
          >
            {badge.text}
          </span>
        )}
      </div>

      {subtitle && <p className="text-xs text-[#A0A8B5] mt-1 truncate">{subtitle}</p>}
    </div>
  );
};

