/**
 * Utility functions to replace optional chaining for browser compatibility
 */

// Helper function to safely access nested properties
function safeAccess(obj, path) {
    const keys = path.split('.');
    let current = obj;

    for (const key of keys) {
        if (current == null) return undefined;
        current = current[key];
    }

    return current;
}

// Helper function for safe property access with default value
function safeGet(obj, path, defaultValue = undefined) {
    const result = safeAccess(obj, path);
    return result !== undefined ? result : defaultValue;
}

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { safeAccess, safeGet };
} else {
    window.safeAccess = safeAccess;
    window.safeGet = safeGet;
}

console.log('🔧 [UTILS] Optional chaining compatibility helpers loaded');