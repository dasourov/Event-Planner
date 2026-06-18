import React from 'react';

interface EventItem {
  id: string;
  title: string;
  description: string;
  location: string;
  latitude?: number;
  longitude?: number;
  date: string;
  categoryId: string;
  categoryName: string;
  maxAttendees?: number;
  attendeeCount: number;
  status: string;
  organizerId: string;
  organizerName: string;
  imageUrl?: string;
}

interface EventCardProps {
  ev: EventItem;
  onViewDetails: (id: string) => void;
}

export const EventCard: React.FC<EventCardProps> = ({ ev, onViewDetails }) => {
  const getCategoryMeta = (catName: string) => {
    const name = catName?.toLowerCase() || '';
    if (name.includes('tech')) return { tag: 'Tech', badgeClass: 'bg-[#4648d4]/10 text-[#4648d4]' };
    if (name.includes('sport')) return { tag: 'Sports', badgeClass: 'bg-emerald-100 text-emerald-800' };
    if (name.includes('music')) return { tag: 'Music', badgeClass: 'bg-pink-100 text-pink-700' };
    if (name.includes('art')) return { tag: 'Art', badgeClass: 'bg-orange-100 text-orange-800' };
    if (name.includes('network')) return { tag: 'Networking', badgeClass: 'bg-blue-100 text-blue-800' };
    if (name.includes('wellness')) return { tag: 'Wellness', badgeClass: 'bg-teal-100 text-teal-800' };
    if (name.includes('workshop')) return { tag: 'Workshop', badgeClass: 'bg-amber-100 text-amber-800' };
    if (name.includes('food')) return { tag: 'Food', badgeClass: 'bg-rose-100 text-rose-800' };
    return { tag: catName || 'Event', badgeClass: 'bg-slate-100 text-slate-700' };
  };

  const meta = getCategoryMeta(ev.categoryName);
  const dateObj = new Date(ev.date);

  const monthStr = dateObj.toLocaleDateString(undefined, { month: 'short' }).toUpperCase();
  const dayStr = dateObj.toLocaleDateString(undefined, { day: '2-digit' });
  const timeStr = dateObj.toLocaleTimeString(undefined, { hour: 'numeric', minute: '2-digit' });
  const endHour = new Date(dateObj.getTime() + 4 * 60 * 60 * 1000);
  const endStr = endHour.toLocaleTimeString(undefined, { hour: 'numeric', minute: '2-digit' });

  return (
    <div
      onClick={() => onViewDetails(ev.id)}
      className="group bg-white rounded-2xl border border-slate-200 hover:border-slate-300 shadow-xs hover:shadow-lg transition-all duration-300 flex flex-col cursor-pointer overflow-hidden transform hover:-translate-y-0.5"
    >
      {ev.imageUrl && (
        <div className="h-44 relative overflow-hidden bg-slate-50 shrink-0">
          <img
            src={ev.imageUrl}
            alt={ev.title}
            className="w-full h-full object-cover group-hover:scale-102 transition-transform duration-500"
          />

          <div className="absolute top-4 left-4 bg-white rounded-xl py-1.5 px-3 flex flex-col items-center justify-center min-w-10 border border-slate-100/50 shadow-xs">
            <span className="text-[9px] font-extrabold text-[#4648d4] tracking-wide leading-none">{monthStr}</span>
            <span className="text-base font-extrabold text-slate-800 tracking-tight leading-tight mt-0.5">{dayStr}</span>
          </div>
        </div>
      )}

      <div className="p-5 flex-1 flex flex-col justify-between space-y-3">
        {!ev.imageUrl && (
          <div className="mb-2">
            <div className="bg-slate-50 rounded-xl py-1.5 px-3 inline-flex flex-col items-center justify-center min-w-10 border border-slate-100/50 shadow-xs">
              <span className="text-[9px] font-extrabold text-[#4648d4] tracking-wide leading-none">{monthStr}</span>
              <span className="text-base font-extrabold text-slate-800 tracking-tight leading-tight mt-0.5">{dayStr}</span>
            </div>
          </div>
        )}
        <div className="space-y-2 text-left">
          <div className="flex items-center gap-2">
            <span className={`px-2 py-0.5 rounded text-[9px] font-extrabold uppercase ${meta.badgeClass}`}>
              {meta.tag}
            </span>
            <span className="text-[10px] text-slate-400 font-bold">
              {timeStr} - {endStr}
            </span>
          </div>

          <h4 className="text-sm font-extrabold text-slate-900 group-hover:text-[#4648d4] transition-colors leading-snug line-clamp-1">
            {ev.title}
          </h4>

          <div className="flex items-center gap-1.5 text-xs text-slate-400 font-medium">
            <span className="material-symbols-outlined text-sm text-[#4648d4]">location_on</span>
            <span className="truncate">{ev.location}</span>
          </div>
        </div>

        <div className="pt-3 border-t border-slate-100 flex items-center justify-between gap-4">
          <span className="text-[10px] font-bold text-emerald-600 uppercase tracking-wide">Free to join</span>
          <span className="text-[10px] text-slate-400 font-semibold">{ev.attendeeCount} attending</span>
        </div>
      </div>
    </div>
  );
};
