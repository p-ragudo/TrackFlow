import React from 'react';
import { StyleSheet, Text, View, Button } from 'react-native';
import { Template } from '@/app/types/Template';

interface TemplateContainerProps {
    template: Template
}

export default function TemplateContainer({template}: TemplateContainerProps) {
    return (
        <View>
            <Text>{template.name}</Text>
            <Text>{template.amount}</Text>
            <Text>{template.category}</Text>
            <Text>{template.tag}</Text>
        </View>
    )
}

const styles = StyleSheet.create({
    
})