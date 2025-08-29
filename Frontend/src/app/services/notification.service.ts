import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {

  // Toast notification
  showToast(message: string, type: 'success' | 'error' | 'info' = 'success'): void {
    // Create toast element and append to body for proper positioning
    const toast = document.createElement('div');
    toast.className = `toast-notification toast-${type}`;
    toast.innerHTML = `
      <div class="toast-content">
        <i class="${type === 'success' ? 'fas fa-check-circle' : (type === 'error' ? 'fas fa-exclamation-circle' : 'fas fa-info-circle')}"></i>
        <span>${message}</span>
      </div>
    `;
    
    // Apply styles directly
    Object.assign(toast.style, {
      position: 'fixed',
      top: '20px',
      left: '50%',
      transform: 'translateX(-50%)',
      zIndex: '10000',
      padding: '12px 20px',
      borderRadius: '8px',
      boxShadow: '0 4px 12px rgba(0, 0, 0, 0.15)',
      cursor: 'pointer',
      fontSize: '0.9rem',
      fontWeight: '500',
      color: 'white',
      background: type === 'success' ? 'linear-gradient(135deg, #10b981 0%, #059669 100%)' : 
                 type === 'error' ? 'linear-gradient(135deg, #ef4444 0%, #dc2626 100%)' :
                 'linear-gradient(135deg, #3b82f6 0%, #2563eb 100%)',
      animation: 'slideDown 0.3s ease-out'
    });
    
    // Style the content
    const content = toast.querySelector('.toast-content') as HTMLElement;
    if (content) {
      Object.assign(content.style, {
        display: 'flex',
        alignItems: 'center',
        gap: '8px'
      });
    }
    
    document.body.appendChild(toast);
    
    // Click to hide
    toast.addEventListener('click', () => {
      document.body.removeChild(toast);
    });
    
    // Auto hide after 3 seconds
    setTimeout(() => {
      if (document.body.contains(toast)) {
        document.body.removeChild(toast);
      }
    }, 3000);
  }

  // Confirmation modal
  showConfirmation(title: string, message: string): Promise<boolean> {
    return new Promise((resolve) => {
      // Create modal and append to body for proper positioning
      const overlay = document.createElement('div');
      overlay.className = 'confirm-modal-overlay';
      
      overlay.innerHTML = `
        <div class="confirm-modal">
          <div class="confirm-header">
            <h3>${title}</h3>
          </div>
          <div class="confirm-body">
            <p>${message}</p>
          </div>
          <div class="confirm-actions">
            <button class="confirm-btn confirm">Evet</button>
            <button class="confirm-btn cancel">İptal</button>
          </div>
        </div>
      `;
      
      // Apply overlay styles
      Object.assign(overlay.style, {
        position: 'fixed',
        top: '0',
        left: '0',
        right: '0',
        bottom: '0',
        background: 'rgba(0, 0, 0, 0.6)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        zIndex: '9999',
        animation: 'fadeIn 0.2s ease-out'
      });
      
      // Apply modal styles
      const modal = overlay.querySelector('.confirm-modal') as HTMLElement;
      if (modal) {
        Object.assign(modal.style, {
          background: 'white',
          borderRadius: '12px',
          boxShadow: '0 10px 25px rgba(0, 0, 0, 0.25)',
          minWidth: '320px',
          maxWidth: '400px',
          animation: 'scaleIn 0.2s ease-out'
        });
      }
      
      // Style header
      const header = overlay.querySelector('.confirm-header') as HTMLElement;
      if (header) {
        Object.assign(header.style, {
          padding: '20px 20px 0'
        });
        const h3 = header.querySelector('h3') as HTMLElement;
        if (h3) {
          Object.assign(h3.style, {
            margin: '0',
            fontSize: '1.2rem',
            fontWeight: '600',
            color: '#1f2937'
          });
        }
      }
      
      // Style body
      const body = overlay.querySelector('.confirm-body') as HTMLElement;
      if (body) {
        Object.assign(body.style, {
          padding: '15px 20px 20px'
        });
        const p = body.querySelector('p') as HTMLElement;
        if (p) {
          Object.assign(p.style, {
            margin: '0',
            color: '#6b7280',
            lineHeight: '1.5'
          });
        }
      }
      
      // Style actions
      const actions = overlay.querySelector('.confirm-actions') as HTMLElement;
      if (actions) {
        Object.assign(actions.style, {
          padding: '0 20px 20px',
          display: 'flex',
          gap: '10px',
          justifyContent: 'flex-end'
        });
        
        const buttons = actions.querySelectorAll('.confirm-btn') as NodeListOf<HTMLElement>;
        buttons.forEach(btn => {
          Object.assign(btn.style, {
            padding: '8px 16px',
            border: 'none',
            borderRadius: '6px',
            fontSize: '0.9rem',
            fontWeight: '500',
            cursor: 'pointer',
            transition: 'all 0.2s ease'
          });
          
          if (btn.classList.contains('cancel')) {
            Object.assign(btn.style, {
              background: '#f3f4f6',
              color: '#6b7280'
            });
          } else {
            Object.assign(btn.style, {
              background: 'linear-gradient(135deg, #ef4444 0%, #dc2626 100%)',
              color: 'white'
            });
          }
        });
      }
      
      document.body.appendChild(overlay);
      
      // Event handlers
      const cancelBtn = overlay.querySelector('.cancel') as HTMLElement;
      const confirmBtn = overlay.querySelector('.confirm') as HTMLElement;
      
      const cleanup = () => {
        if (document.body.contains(overlay)) {
          document.body.removeChild(overlay);
        }
      };
      
      // Cancel handlers
      overlay.addEventListener('click', () => {
        cleanup();
        resolve(false);
      });
      modal?.addEventListener('click', (e) => e.stopPropagation());
      cancelBtn?.addEventListener('click', () => {
        cleanup();
        resolve(false);
      });
      
      // Confirm handler
      confirmBtn?.addEventListener('click', () => {
        cleanup();
        resolve(true);
      });
    });
  }
}
