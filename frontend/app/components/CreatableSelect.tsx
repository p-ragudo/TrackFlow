import React, { useState, useRef } from 'react';
import {
  StyleSheet,
  Text,
  View,
  TextInput,
  TouchableOpacity,
  ScrollView,
  Keyboard,
  KeyboardAvoidingView,
  Platform,
} from 'react-native';

interface CreatableSelectProps {
  label: string
  value: string
  onChangeText: (text: string) => void
  options: string[]
  placeholder?: string
}

export default function CreatableSelect({
    label,
    value,
    onChangeText,
    options,
    placeholder
}: CreatableSelectProps) {
    const [isOpen, setIsOpen] = useState(false);
    const inputRef = useRef<TextInput>(null);

    // Filter options based on user input
    const filteredOptions = options.filter((item) =>
        item.toLowerCase().includes(value.toLowerCase()))

    const toggleDropdown = () => {
        if (isOpen) {
            setIsOpen(false);
            Keyboard.dismiss();
        } else {
            setIsOpen(true);
            inputRef.current?.focus();
        }
    };

    return (
        <View style={styles.fieldContainer}>
            <Text style={styles.label}>{label}</Text>
            
            <View>
                <TextInput
                    style={styles.input}
                    value={value}
                    onChangeText={(text) => {
                    onChangeText(text);
                    setIsOpen(true);
                    }}
                    onFocus={() => setIsOpen(true)}
                    onBlur={() => setTimeout(() => setIsOpen(false), 150)}
                    placeholder={placeholder}
                    placeholderTextColor="#A0A0A0"
                />
                
                <TouchableOpacity
                    style={styles.arrowContainer}
                    onPress={toggleDropdown}
                    activeOpacity={0.7}
                >
                    <Text style={[styles.arrow, isOpen && styles.arrowOpen]}>▼</Text>
                </TouchableOpacity>
            </View>

            {/* Suggestion Dropdown */}
            {isOpen && filteredOptions.length > 0 && (
                <ScrollView 
                    nestedScrollEnabled={true} 
                    style={styles.dropdown}
                >
                    {filteredOptions.map((item, index) => (
                    <TouchableOpacity
                        key={index}
                        style={styles.dropdownOption}
                        onPress={() => {
                        onChangeText(item);
                        setIsOpen(false);
                        }}
                    >
                        <Text style={styles.dropdownText}>{item}</Text>
                    </TouchableOpacity>
                    ))}
                </ScrollView>
            )}
        </View>
    )
}

const styles = StyleSheet.create({
    fieldContainer: {
        position: 'relative',
        zIndex: 1000, // Ensures dropdown overlays items below it
    },
    inputWrapper: {
        position: 'relative',
        justifyContent: 'center',
    },
    label: {
        fontSize: 14,
        fontWeight: '600',
        color: '#000000',
        marginBottom: 6,
    },
    input: {
        height: 44,
        borderWidth: 0.5,
        borderColor: '#8E8E8E',
        borderRadius: 8,
        paddingHorizontal: 12,
        fontSize: 15,
        color: '#000000',
        backgroundColor: '#FFFFFF',
    },
    arrowContainer: {
        position: 'absolute',
        top: 10,
        right: 14,
        alignItems: 'center',
        justifyContent: 'center',
    },
    arrow: {
        fontSize: 16,
        color: '#666666',
    },
    arrowOpen: {
        transform: [{ rotate: '180deg' }], // Flips arrow upward when dropdown is open
    },
    dropdown: {
        position: 'absolute',
        top: 70,
        left: 0,
        right: 0,
        backgroundColor: '#FFFFFF',
        borderWidth: 1,
        borderColor: '#CCCCCC',
        borderRadius: 6,
        zIndex: 1000,
        overflow: 'scroll',
        elevation: 5,
        shadowColor: '#000000',
        shadowOffset: { width: 0, height: 2 },
        shadowOpacity: 0.1,
        shadowRadius: 4,
    },
    dropdownOption: {
        paddingVertical: 10,
        paddingHorizontal: 12,
        borderBottomWidth: 1,
        borderBottomColor: '#F0F0F0',
    },
    dropdownText: {
        fontSize: 14,
        color: '#333333',
    }
})