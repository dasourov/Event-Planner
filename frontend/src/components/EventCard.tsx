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
}

interface EventCardProps {
  ev: EventItem;
  isLiked: boolean;
  onToggleLike: (id: string, e: React.MouseEvent) => void;
  onViewDetails: (id: string) => void;
}

export const EventCard: React.FC<EventCardProps> = ({
  ev,
  isLiked,
  onToggleLike,
  onViewDetails
}) => {

  // Category tags and images mapper matching mockup photo
  const getCategoryMeta = (catName: string) => {
    const name = catName?.toLowerCase() || '';
    if (name.includes('tech')) {
      return {
        tag: 'Tech',
        badgeClass: 'bg-[#4648d4]/10 text-[#4648d4]',
        image: 'https://images.unsplash.com/photo-1540575467063-178a50c2df87?w=600&auto=format&fit=crop&q=80',
        priceSim: '$149.00'
      };
    }
    if (name.includes('sport') || name.includes('wellness')) {
      return {
        tag: 'Wellness',
        badgeClass: 'bg-emerald-100 text-emerald-800',
        image: 'https://images.unsplash.com/photo-1506126613408-eca07ce68773?w=600&auto=format&fit=crop&q=80',
        priceSim: 'Free'
      };
    }
    if (name.includes('music') || name.includes('art') && name.includes('festival')) {
      return {
        tag: 'Arts',
        badgeClass: 'bg-orange-100 text-orange-800',
        image: 'https://images.unsplash.com/photo-1492684223066-81342ee5ff30?w=600&auto=format&fit=crop&q=80',
        priceSim: 'Free'
      };
    }
    if (name.includes('art')) {
      return {
        tag: 'Arts',
        badgeClass: 'bg-orange-100 text-orange-800',
        image: 'https://images.unsplash.com/photo-1513364776144-60967b0f800f?w=600&auto=format&fit=crop&q=80',
        priceSim: '$45.00'
      };
    }
    if (name.includes('networking')) {
      return {
        tag: 'Networking',
        badgeClass: 'bg-[#4648d4]/10 text-[#4648d4]',
        image: 'https://images.unsplash.com/photo-1511578314322-379afb476865?w=600&auto=format&fit=crop&q=80',
        priceSim: '$25.00'
      };
    }
    if (name.includes('workshop')) {
      return {
        tag: 'Workshop',
        badgeClass: 'bg-amber-100 text-amber-800',
        image: 'https://images.unsplash.com/photo-1531403009284-440f080d1e12?w=600&auto=format&fit=crop&q=80',
        priceSim: '$299.00'
      };
    }
    // Default
    return {
      tag: 'Wellness',
      badgeClass: 'bg-slate-100 text-slate-700',
      image: 'https://images.unsplash.com/photo-1506126613408-eca07ce68773?w=600&auto=format&fit=crop&q=80',
      priceSim: 'Free'
    };
  };

  const meta = getCategoryMeta(ev.categoryName);
  const dateObj = new Date(ev.date);
  
  // Date details
  const monthStr = dateObj.toLocaleDateString(undefined, { month: 'short' }).toUpperCase();
  const dayStr = dateObj.toLocaleDateString(undefined, { day: '2-digit' });

  // Time details
  const timeStr = dateObj.toLocaleTimeString(undefined, { hour: 'numeric', minute: '2-digit' });
  const endHour = new Date(dateObj.getTime() + 4 * 60 * 60 * 1000); // 4-hour simulated length
  const endStr = endHour.toLocaleTimeString(undefined, { hour: 'numeric', minute: '2-digit' });

  return (
    <div
      onClick={() => onViewDetails(ev.id)}
      className="group bg-white rounded-2xl border border-slate-200 hover:border-slate-300 shadow-xs hover:shadow-lg transition-all duration-300 flex flex-col cursor-pointer overflow-hidden transform hover:-translate-y-0.5"
    >
      {/* Top Image portion */}
      <div className="h-44 relative overflow-hidden bg-slate-50 shrink-0">
        <img 
          src={meta.image} 
          alt={ev.title}
          className="w-full h-full object-cover group-hover:scale-102 transition-transform duration-500" 
        />
        
        {/* Date badge */}
        <div className="absolute top-4 left-4 bg-white rounded-xl py-1.5 px-3 flex flex-col items-center justify-center min-w-10 border border-slate-100/50 shadow-xs">
          <span className="text-[9px] font-extrabold text-[#4648d4] tracking-wide leading-none">{monthStr}</span>
          <span className="text-base font-extrabold text-slate-800 tracking-tight leading-tight mt-0.5">{dayStr}</span>
        </div>

        {/* Heart button */}
        <button
          onClick={(e) => onToggleLike(ev.id, e)}
          className="absolute top-4 right-4 w-9 h-9 rounded-full bg-white flex items-center justify-center border border-slate-100/50 shadow-xs text-slate-400 hover:text-red-500 transition-colors"
        >
          <span className={`material-symbols-outlined text-lg ${isLiked ? 'text-red-500 fill-current' : ''}`}>
            favorite
          </span>
        </button>
      </div>

      {/* Details portion */}
      <div className="p-5 flex-1 flex flex-col justify-between space-y-3">
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

        {/* Footer */}
        <div className="pt-3 border-t border-slate-100 flex items-center justify-between gap-4">
          <span className="text-xs font-extrabold text-slate-800">
            {meta.priceSim}
          </span>
          <button
            onClick={(e) => { e.stopPropagation(); onViewDetails(ev.id); }}
            className="px-3 py-1.5 bg-[#4648d4]/10 hover:bg-[#4648d4]/20 text-[#4648d4] font-bold text-[10px] rounded-lg transition-all"
          >
            View Details
          </button>
        </div>
      </div>
    </div>
  );
};
